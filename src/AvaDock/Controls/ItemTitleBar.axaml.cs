// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// A item header that initiates window controls for for <see cref="DockItem"/>s.
    /// </summary>
    public partial class ItemTitleBar : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemTitleBar"/> class.
        /// </summary>
        public ItemTitleBar()
        {
            this.InitializeComponent();

            // Rebuild the context menu whenever DataContext changes or the control is attached
            this.DataContextChanged += (_, __) => this.UpdateContextMenu();
            this.AttachedToVisualTree += (_, __) => this.UpdateContextMenu();
        }

        /// <summary>
        /// Handles the <see cref="MenuItem.Click"/> event for the <c>Float</c> context menu item.
        /// </summary>
        /// <param name="sender">The menu item that raised the event.</param>
        /// <param name="eventArgs">The event data associated with the click.</param>
        private void OnFloatMenuClick(Object? sender, RoutedEventArgs eventArgs)
        {
            if (this.DataContext is DockItemViewModel item)
            {
                this.OnFloatMenuClicked(item);
            }
        }

        /// <summary>
        /// Handles the <c>Float</c> menu item click for a <see cref="DockItemViewModel"/> header.
        /// </summary>
        /// <param name="item">
        /// The <see cref="DockItemViewModel"/> associated with the item whose context menu was used.
        /// </param>
        private void OnFloatMenuClicked(DockItemViewModel item)
        {
            DockControlManager? host = DockContext.GetDockHost(item);

            DockItem? dockItem = this.FindAncestorOfType<DockItem>();

            if (dockItem is not null)
            {
                PixelPoint topLeft = dockItem.PointToScreen(new Point(0, 0));
                // DesiredSize excludes the ItemHeader and results in an area that is too small.
                Size size = new(dockItem.Bounds.Width, dockItem.Bounds.Height);
                host?.FloatItem(item, topLeft, size);
            }
        }

        /// <summary>
        /// Recreate the ContextMenu based on the current DataContext.
        /// </summary>
        private void UpdateContextMenu()
        {
            if (this.DataContext is not DockItemViewModel item)
            {
                this.ContextMenu = null;
                return;
            }

            MenuItem menuItem = new()
            {
                Header = "Float",
            };

            // Prevent double-subscribe
            menuItem.Click -= this.OnFloatMenuClick;
            menuItem.Click += this.OnFloatMenuClick;

            ContextMenu contextMenu = this.FindControl<ContextMenu>("PART_ContextMenu") ?? new ContextMenu();
            contextMenu.Items.Clear();
            _ = contextMenu.Items.Add(menuItem);

            this.ContextMenu = contextMenu;
        }
    }
}
