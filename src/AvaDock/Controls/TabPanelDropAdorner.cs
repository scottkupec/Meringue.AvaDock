// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// An adorner layer for dock drop locations.
    /// </summary>
    public class TabPanelDropAdorner : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabPanelDropAdorner"/> class.
        /// </summary>
        public TabPanelDropAdorner()
        {
            this.Focusable = false;
            this.IsHitTestVisible = false;

            if (Application.Current?.TryFindResource("AvaDockAccentColor", Application.Current?.ActualThemeVariant, out Object? resource) == true &&
                resource is Color accentColor)
            {
                this.Brush = new SolidColorBrush(accentColor, 0.3);
                this.Pen = new Pen(this.Brush, 2);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Theme resource 'AvaDockAccentColor' was not found.");

                this.Brush = new SolidColorBrush(Colors.Gray, 0.3);
                this.Pen = new Pen(Brushes.Gray, 2);
            }
        }

        /// <summary>Gets or sets the <see cref="Control"/> being adorned.</summary>
        public Control? AdornedElement { get; set; }

        /// <summary>Gets the current <see cref="DropZone"/> being hovered over.</summary>
        public DropZone HoveredZone => this.DropTarget.CurrentZone;

        /// <summary>Gets the <see cref="Brush"/> for the highlight <see cref="Rect"/>.</summary>
        private SolidColorBrush Brush { get; }

        /// <summary>Gets the <see cref="DropZoneBounds"/> for the current instance.</summary>
        private DropZoneBounds DropTarget { get; } = new();

        /// <summary>Gets the <see cref="Pen"/> for the highlight <see cref="Rect"/>.</summary>
        private Pen Pen { get; }

        /// <summary>Renders the adorner layer.</summary>
        /// <param name="context">The context over which the layer should be drawn.</param>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (context is not null && this.AdornedElement is not null)
            {
                Rect highlightRect = this.DropTarget.HighlightRect;

                if (highlightRect != default)
                {
                    context.DrawRectangle(this.Brush, this.Pen, highlightRect);
                }
            }
        }

        /// <summary>Sets the value of <see cref="Visual.IsVisible"/> for the current instance.</summary>
        /// <param name="isVisible">The new value for the <see cref="Visual.IsVisible"/>.</param>
        /// <remarks>Added as a null-colaesce friendly setter.</remarks>
        public void SetVisible(Boolean isVisible)
        {
            this.IsVisible = isVisible;
        }

        /// <summary>
        /// Updates the <see cref="Control"/> being adorned.
        /// </summary>
        /// <param name="adornedElement">The control being adorned.</param>
        /// <param name="pointerPosition">The new pointer location.</param>
        // CONSIDER: Move adornedElement to constructor.
        public void UpdateTarget(Control adornedElement, Point pointerPosition)
        {
            this.AdornedElement = adornedElement ?? throw new ArgumentNullException(nameof(adornedElement));

            Matrix? transform = adornedElement.TransformToVisual(this);
            if (transform.HasValue)
            {
                Rect localBounds = adornedElement.Bounds;

                Point topLeft = transform.Value.Transform(localBounds.TopLeft);
                Point bottomRight = transform.Value.Transform(localBounds.BottomRight);
                Rect boundsInAdornerSpace = new(topLeft, bottomRight);

                this.DropTarget.UpdateBounds(boundsInAdornerSpace);

                Point pointerInAdornerSpace = transform.Value.Transform(pointerPosition);
                if (this.DropTarget.UpdatePointer(pointerInAdornerSpace))
                {
                    this.InvalidateVisual();
                }
            }
        }

        /// <summary>Encapulates DropZone management.</summary>
        /// <remarks>This can be made private one we update to Avalonia 12 and update the Render() tests.</remarks>
        protected class DropZoneBounds
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DropZoneBounds"/> class.
            /// </summary>
            public DropZoneBounds()
            {
                this.ZoneHighlights[DropZone.None] = default;
            }

            /// <summary>Gets the current <see cref="DropZone"/>.</summary>
            public DropZone CurrentZone { get; private set; } = DropZone.None;

            /// <summary>Gets the highlight <see cref="Rect"/> for the <see cref="CurrentZone"/>.</summary>
            public Rect HighlightRect => this.ZoneHighlights[this.CurrentZone];

            /// <summary>Gets or sets the last bounding <see cref="Rect"/> used.</summary>
            private Rect? LastBounds { get; set; }

            /// <summary>Gets or sets the last pointer position checked.</summary>
            private Point? LastPointerPosition { get; set; }

            /// <summary>Gets the <see cref="Rect"/>s for each <see cref="DropZone"/>.</summary>
            private Dictionary<DropZone, Rect> ZoneHighlights { get; } = [];

            /// <summary>Gets pointer position thresholds for possible <see cref="DropZone"/>s.</summary>
            private Dictionary<DropZone, Double> ZoneThresholds { get; } = [];

            /// <summary>Updates the containing bounds for the drop zones.</summary>
            /// <param name="bounds">The new containering bounds.</param>
            public void UpdateBounds(Rect bounds)
            {
                if (!this.LastBounds.HasValue || this.LastBounds.Value != bounds)
                {
                    this.LastBounds = bounds;

                    Double left = bounds.Left;
                    Double top = bounds.Top;
                    Double width = bounds.Width;
                    Double height = bounds.Height;

                    this.ZoneHighlights[DropZone.Bottom] = new Rect(
                        left,
                        top + (height * 0.5),
                        width,
                        height * 0.5);

                    this.ZoneHighlights[DropZone.Center] = new Rect(
                        left + (width * 0.25),
                        top + (height * 0.25),
                        width * 0.5,
                        height * 0.5);

                    this.ZoneHighlights[DropZone.Left] = new(
                        left,
                        top,
                        width * 0.5,
                        height);

                    this.ZoneHighlights[DropZone.Right] = new(
                        left + (width * 0.5),
                        top,
                        width * 0.5,
                        height);

                    this.ZoneHighlights[DropZone.Top] = new(
                        left,
                        top,
                        width,
                        height * 0.5);

                    this.ZoneThresholds[DropZone.Bottom] = top + (height * 0.75);
                    this.ZoneThresholds[DropZone.Left] = left + (width * 0.25);
                    this.ZoneThresholds[DropZone.Top] = top + (height * 0.25);
                    this.ZoneThresholds[DropZone.Right] = left + (width * 0.75);
                }
            }

            /// <summary>Updates the current <see cref="HighlightRect"/> based on the provided pointer position.</summary>
            /// <param name="pointerLocation">The new pointer position.</param>
            /// <returns><c>true</c> if <see cref="CurrentZone"/> changed; otherwise, <c>false</c>.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Not more readable here.")]
            public Boolean UpdatePointer(Point? pointerLocation)
            {
                Boolean updated = false;

                if (this.LastPointerPosition != pointerLocation)
                {
                    this.LastPointerPosition = pointerLocation;

                    DropZone newDropZone = DropZone.None;

                    if (pointerLocation is { } position && this.LastBounds.HasValue)
                    {
                        if (!this.LastBounds.Value.Contains(position))
                        {
                            newDropZone = DropZone.None;
                        }
                        else if (position.X < this.ZoneThresholds[DropZone.Left])
                        {
                            newDropZone = DropZone.Left;
                        }
                        else if (position.X > this.ZoneThresholds[DropZone.Right])
                        {
                            newDropZone = DropZone.Right;
                        }
                        else if (position.Y < this.ZoneThresholds[DropZone.Top])
                        {
                            newDropZone = DropZone.Top;
                        }
                        else if (position.Y > this.ZoneThresholds[DropZone.Bottom])
                        {
                            newDropZone = DropZone.Bottom;
                        }
                        else
                        {
                            newDropZone = DropZone.Center;
                        }
                    }

                    if (this.CurrentZone != newDropZone)
                    {
                        this.CurrentZone = newDropZone;
                        updated = true;
                    }
                }

                return updated;
            }
        }
    }
}
