// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Defines the visuals for a <see cref="DockItemViewModel"/>. Generally, this is presented
    /// as a child of a <see cref="DockTabPanel"/>, but may be shown minimized, temporarily as
    /// an overlay, or as a maximized overlay.
    /// </summary>
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(Border))]
    public partial class DockItem : ContentControl
    {
        /// <summary>Defines the <see cref="IsActive"/> property for binding.</summary>
        public static readonly StyledProperty<Boolean> IsActiveProperty =
            AvaloniaProperty.Register<DockItem, Boolean>(nameof(IsActive));

        /// <summary>Defines the <see cref="Title"/> property for binding.</summary>
        public static readonly StyledProperty<String> TitleProperty =
            AvaloniaProperty.Register<DockItem, String>(nameof(Title));

        /// <summary>The point where the pointer was initially pressed.</summary>
        private Point? dragStartPoint;

        /// <summary>Indicates whether a drag operation is in progress.</summary>
        private Boolean isDragging;

        /// <summary>The pointer pressed event args captured at drag start.</summary>
        private PointerPressedEventArgs? dragStartEventArgs;

        /// <summary>
        /// Gets or sets a value indicating whether or not the item is active.
        /// </summary>
        public Boolean IsActive
        {
            get => this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        /// <summary>
        /// Gets or sets the title for the item.
        /// </summary>
        public String Title
        {
            get => this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (e?.NameScope.Find<Border>("PART_TitleBar") is { } titleBar)
            {
                titleBar.PointerMoved += this.OnTitleBarPointerMovedAsync;
                titleBar.PointerPressed += this.OnTitleBarPointerPressed;
                titleBar.PointerReleased += this.OnTitleBarPointerReleased;
            }

            this.AddHandler(InputElement.PointerPressedEvent, this.OnDockItemPointerPressed, handledEventsToo: true);
            this.AddHandler(InputElement.GotFocusEvent, this.OnDockItemGotFocus, RoutingStrategies.Bubble, handledEventsToo: true);
        }

        /// <inheritdoc/>
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (this.DataContext is DockItemViewModel)
            {
                // Bind IsActive to DataContext.Active. Binding tracks DataContext changes automatically,
                // so we don't need to unbind manually.
                _ = this.Bind(
                    DockItem.IsActiveProperty,
                    new Binding(nameof(DockItemViewModel.Active)));
            }
        }

        /// <summary>
        /// Handles the item getting focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnDockItemGotFocus(Object? sender, RoutedEventArgs eventArgs)
        {
            if (this.DataContext is DockItemViewModel item)
            {
                item.MakeActiveCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handles pointer pressed events to activate the item.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        private void OnDockItemPointerPressed(Object? sender, PointerPressedEventArgs eventArgs)
        {
            if (this.DataContext is DockItemViewModel item)
            {
                item.MakeActiveCommand?.Execute(null);
            }
        }

        /// <summary>Processes <see cref="PointerPressedEventArgs"/> events when the pointer is pressed.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        // TODO: Refactor to use an injectable interface for DragDrop so tests can inspect that the correct
        //       DragDrop operation is initiated.  Update PointerPressed_InitiatesDragDrop test to use that.
        private void OnTitleBarPointerPressed(Object? sender, PointerPressedEventArgs eventArgs)
        {
            if (this.DataContext is DockItemViewModel item
                && eventArgs.Pointer.Type == PointerType.Mouse
                && eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                item.MakeActiveCommand?.Execute(null);
                this.dragStartPoint = eventArgs.GetPosition(this);
                this.isDragging = false;
                this.dragStartEventArgs = eventArgs;
            }
        }

        /// <summary>Processes <see cref="PointerEventArgs"/> events when the pointer is moved over the title bar.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        private async void OnTitleBarPointerMovedAsync(Object? sender, PointerEventArgs eventArgs)
        {
            if (this.DataContext is not DockItemViewModel item
                || this.dragStartPoint is null
                || this.dragStartEventArgs is null
                || !eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                return;
            }

            Point pos = eventArgs.GetPosition(this);
            Point delta = pos - this.dragStartPoint.Value;

            if (!this.isDragging && (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4))
            {
                this.isDragging = true;

                DataTransferItem itemData = new();
                itemData.Set(DockContext.DockItemDragFormat, item);

#pragma warning disable CA2000 // Avalonia disposes the DataTransfer. See remarks on https://github.com/AvaloniaUI/Avalonia/blob/main/src/Avalonia.Base/Input/IDataTransfer.cs
                DataTransfer dataTransfer = new();
                dataTransfer.Add(itemData);
#pragma warning restore CA2000 // Dispose objects before losing scope

                _ = await DragDrop.DoDragDropAsync(this.dragStartEventArgs, dataTransfer, DragDropEffects.Move).ConfigureAwait(false);
            }
        }

        /// <summary>Processes <see cref="PointerReleasedEventArgs"/> events when the pointer is released.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        private void OnTitleBarPointerReleased(Object? sender, PointerReleasedEventArgs eventArgs)
        {
            this.dragStartPoint = null;
            this.isDragging = false;
            this.dragStartEventArgs = null;
        }
    }
}
