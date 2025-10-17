// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Styling;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Adorner for showing a visual insert indicator between tabs when reordering.
    /// </summary>
    public class TabReorderAdorner : Control
    {
        /// <summary>The size for the <see cref="Pen"/> used to draw the highlight line.</summary>
        private const Int32 PenSize = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabReorderAdorner"/> class.
        /// </summary>
        public TabReorderAdorner()
        {
            this.IsHitTestVisible = false;
        }

        /// <summary>Gets or sets item in <see cref="TargetPanel"/> being hovered over.</summary>
        private Int32 HoverIndex { get; set; }

        /// <summary>Gets or sets the target tab strip being adorned.</summary>
        private DockTabPanel? TargetPanel { get; set; }

        /// <summary>Sets the value of <see cref="Visual.IsVisible"/> for the current instance.</summary>
        /// <param name="isVisible">The new value for the <see cref="Visual.IsVisible"/>.</param>
        /// <remarks>Added as a null-colaesce friendly setter.</remarks>
        public void SetVisible(Boolean isVisible)
        {
            this.IsVisible = isVisible;
        }

        /// <summary>
        /// Updates which tab panel and index we are highlighting.
        /// </summary>
        /// <param name="panel">The target tab panel.</param>
        /// <param name="index">The index to show the insert line at.</param>
        public void UpdateTarget(DockTabPanel panel, Int32 index)
        {
            this.TargetPanel = panel;
            this.HoverIndex = index;
            this.InvalidateVisual();
        }

        /// <inheritdoc/>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (this.TargetPanel == null || this.TargetPanel.ItemCount == 0 || this.HoverIndex < 0 || context is null)
            {
                return;
            }

            if (this.TargetPanel.ContainerFromIndex(this.HoverIndex) is TabItem tabItem)
            {
                if (tabItem.Presenter is { } header)
                {
                    // Locate the ItemsPresenter (tab strip host)
                    ItemsPresenter? itemsPresenter = this.TargetPanel
                        .GetTemplateChildren()
                        .OfType<ItemsPresenter>()
                        .FirstOrDefault();

                    if (itemsPresenter != null)
                    {
                        // Bounds of the header in strip space
                        Rect headerBounds = header.Bounds;

                        // Transform strip space to adorner space
                        Matrix? transform = header.TransformToVisual(this);
                        if (transform.HasValue)
                        {
                            Rect rectInAdorner = new(
                                transform.Value.Transform(headerBounds.TopLeft),
                                transform.Value.Transform(headerBounds.BottomRight));

                            (Point lineStart, Point lineEnd) = this.GetHighlightLine(itemsPresenter, rectInAdorner);
                            Pen pen = TabReorderAdorner.GetHighlightPen();
                            context.DrawLine(pen, lineStart, lineEnd);
                        }
                    }
                }
            }
        }

        /// <summary>Gets <see cref="Pen"/> to use for drawing the highlight.</summary>
        /// <returns>The <see cref="Pen"/> to use.</returns>
        private static Pen GetHighlightPen()
        {
            Pen pen = new(Brushes.Gray, TabReorderAdorner.PenSize);

            if (Application.Current?.TryFindResource("AvaDockAccentColor", Application.Current?.ActualThemeVariant, out Object? resource) == true)
            {
                System.Diagnostics.Debug.Assert(resource != null, $"Theme resource 'AvaDockAccentColor' was not found.");

                if (resource is Color accentColor)
                {
                    pen = new Pen(new SolidColorBrush(accentColor), TabReorderAdorner.PenSize);
                }
            }

            return pen;
        }

        /// <summary>Gets the start and end <see cref="Point"/>s for the highlight line.</summary>
        /// <param name="itemsPresenter">The item presenter containing the list of tab items.</param>
        /// <param name="itemRect">The rect surrounding the item being hovered over.</param>
        /// <returns>The start and end <see cref="Point"/>s for the line to be drawn.</returns>
        private (Point Start, Point End) GetHighlightLine(ItemsPresenter itemsPresenter, Rect itemRect)
        {
            System.Diagnostics.Debug.Assert(this.TargetPanel != null, $"{nameof(TabReorderAdorner.GetHighlightLine)} called with null {nameof(this.TargetPanel)}.");

            // Transform itemsPresenter.Bounds into adorner space
            Matrix? presenterTransform = itemsPresenter.TransformToVisual(this);
            Rect presenterBounds = itemsPresenter.Bounds;

            if (presenterTransform.HasValue)
            {
                Point topLeft = presenterTransform.Value.Transform(presenterBounds.TopLeft);
                Point bottomRight = presenterTransform.Value.Transform(presenterBounds.BottomRight);
                presenterBounds = new Rect(topLeft, bottomRight);
            }

            Point lineStart = default;
            Point lineEnd = default;

            switch (this.TargetPanel!.TabStripPlacement)
            {
                case Dock.Top:
                case Dock.Bottom:
                    {
                        Double lineHeight = presenterBounds.Height * 0.75;
                        Double centerY = presenterBounds.Top + (presenterBounds.Height / 2);
                        Double halfHeight = lineHeight / 2;

                        lineStart = new(itemRect.Left, centerY - halfHeight);
                        lineEnd = new(itemRect.Left, centerY + halfHeight);
                    }

                    break;

                case Dock.Left:
                case Dock.Right:
                    {
                        Double padding = itemRect.Height * 0.5;
                        Double top = itemRect.Top - padding;
                        Double lineWidth = presenterBounds.Width * 0.75;
                        Double centerX = presenterBounds.Left + (presenterBounds.Width / 2);
                        Double halfWidth = lineWidth / 2;

                        lineStart = new(centerX - halfWidth, top);
                        lineEnd = new(centerX + halfWidth, top);
                    }

                    break;

                default:
                    System.Diagnostics.Debug.Assert(false, $"Unknown {nameof(this.TargetPanel.TabStripPlacement)}.");
                    break;
            }

            return (lineStart, lineEnd);
        }
    }
}
