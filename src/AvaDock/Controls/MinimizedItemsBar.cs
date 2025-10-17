// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Displays stub headers for <see cref="DockWorkspace.MinimizedItems"/> along the <see cref="DockWorkspace"/> edge.
    /// </summary>
    internal class MinimizedItemsBar : TemplatedControl
    {
        /// <summary>
        /// Defines the style property for the <see cref="DockEdge"/> member.
        /// </summary>
        public static readonly StyledProperty<Dock?> DockEdgeProperty =
            AvaloniaProperty.Register<MinimizedItemsBar, Dock?>(nameof(DockEdge));

        /// <summary>
        /// Defines the style property for the <see cref="ShouldRotate"/> member.
        /// </summary>
        public static readonly StyledProperty<Boolean?> ShouldRotateProperty =
            AvaloniaProperty.Register<MinimizedItemsBar, Boolean?>(nameof(ShouldRotate));

        /// <summary>
        /// Defines the style property for the <see cref="Items"/> member.
        /// </summary>
        public static readonly StyledProperty<IEnumerable<DockItemViewModel>?> ItemsProperty =
            AvaloniaProperty.Register<MinimizedItemsBar, IEnumerable<DockItemViewModel>?>(nameof(Items));

        /// <summary>
        /// Gets or sets the window edge the minimized items display against.
        /// </summary>
        public Dock? DockEdge
        {
            get => this.GetValue(DockEdgeProperty);
            set => this.SetValue(DockEdgeProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to rotate headers. If null, defaults to <c>true</c> for Left/Right edges
        /// and <c>false</c> for Top/Bottom edges.
        /// </summary>
        public Boolean? ShouldRotate
        {
            get => this.GetValue(ShouldRotateProperty);
            set => this.SetValue(ShouldRotateProperty, value);
        }

        /// <summary>
        /// Gets or sets the collection of minimized dock items to display.
        /// </summary>
        public IEnumerable<DockItemViewModel>? Items
        {
            get => this.GetValue(ItemsProperty) ?? [];
            set => this.SetValue(ItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets the command executed when the item is selected.
        /// </summary>
        public ICommand ItemSelectedCommand { get; set; } = new RelayCommand<DockItemViewModel>(
            item =>
            {
                if (item is not null && item.RestoreCommand.CanExecute(null))
                {
                    item.RestoreCommand.Execute(null);
                }
            });
    }
}
