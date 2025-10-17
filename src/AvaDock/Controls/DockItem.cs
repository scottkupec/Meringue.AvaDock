// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Meringue.AvaDock.Services;
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
        /// <summary>Defines the <see cref="Title"/> property for binding.</summary>
        public static readonly StyledProperty<String> TitleProperty =
            AvaloniaProperty.Register<DockItem, String>(nameof(Title));

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
                titleBar.PointerPressed += this.OnTitleBarPointerPressedAsync;
            }
        }

        /// <summary>Processes <see cref="PointerPressedEventArgs"/> events when the pointer is pressed.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        // TODO: Refactor to use an injectable interface for DragDrop so tests can inspect that the correct
        //       DragDrop operation is initiated.  Update PointerPressed_InitiatesDragDrop test to use that.
        private async void OnTitleBarPointerPressedAsync(Object? sender, PointerPressedEventArgs eventArgs)
        {
            if (this.DataContext is DockItemViewModel item
                && eventArgs.Pointer.Type == PointerType.Mouse
                && eventArgs.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                DataObject data = new();
                data.Set(DockContext.DragDropContextName, item);
                _ = await DragDrop.DoDragDrop(eventArgs, data, DragDropEffects.Move).ConfigureAwait(false);
                eventArgs.Handled = true;
            }
        }
    }
}
