// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Events;

namespace Meringue.AvaDock.ViewModels
{
    /// <summary>
    /// Defines the view model for use with <see cref="DockItem"/>.
    /// </summary>
    // TODO: Review all ICommand's to cache the commands.  Make the corresponding Disable* properties
    //       observable so the commands can be updated as necessary.
    public partial class DockItemViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the context of the item.
        /// </summary>
        /// <remarks>
        /// This is the control or templated view model being displayed.
        /// </remarks>
        [ObservableProperty]
        private Object? context;

        /// <summary>
        /// Gets the title for the item.
        /// </summary>
        /// <remarks>
        /// This is displayed in the title bar, item list, and tab tile when minimized.  It
        /// may be truncated to fit available space in each of those cases.
        /// </remarks>
        [ObservableProperty]
        private String title = "[Untitled]";

        /// <summary>
        /// Occurs when a request is made to close this item.
        /// </summary>
        public event EventHandler<DockItemCloseRequestedEventArgs>? CloseRequested;

        /// <summary>
        /// Occurs when a request is made to float this item.
        /// </summary>
        public event EventHandler<DockItemFloatRequestedEventArgs>? FloatRequested;

        /// <summary>
        /// Occurs when a request is made to hide this item.
        /// </summary>
        public event EventHandler<DockItemHideRequestedEventArgs>? HideRequested;

        /// <summary>
        /// Occurs when a request is made to maximize this item.
        /// </summary>
        public event EventHandler<DockItemMaximizeRequestedEventArgs>? MaximizeRequested;

        /// <summary>
        /// Occurs when a request is made to minimize this item.
        /// </summary>
        public event EventHandler<DockItemMinimizeRequestedEventArgs>? MinimizeRequested;

        /// <summary>
        /// Occurs when a request is made to restore this item.
        /// </summary>
        public event EventHandler<DockItemRestoreRequestedEventArgs>? RestoreRequested;

        /// <summary>
        /// Occurs when a request is made to show this item.
        /// </summary>
        public event EventHandler<DockItemShowRequestedEventArgs>? ShowRequested;

        /// <summary>
        /// Gets the command that requests the item be closed.
        /// </summary>
        public ICommand CloseCommand => new RelayCommand(this.CloseItem, () => !this.DisableClose);

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being clsoed.
        /// </summary>
        public Boolean DisableClose { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being hidden.
        /// </summary>
        public Boolean DisableHide { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being maximized.
        /// </summary>
        /// <remarks>
        /// Currently not implemented. This is a placeholder so consumers can proactively
        /// set it if they don't want the ability to be maximized to be enabled automatically
        /// once the feature is complete.
        /// </remarks>
        public Boolean DisableMaximize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being minimized.
        /// </summary>
        public Boolean DisableMinimize { get; set; }

        /// <summary>
        /// Gets the command that requests the item be floated.
        /// </summary>
        public ICommand FloatCommand => new RelayCommand(this.FloatItem);

        /// <summary>
        /// Gets the command that requests the item be hidden.
        /// </summary>
        public ICommand HideCommand => new RelayCommand(this.HideItem, () => !this.DisableHide);

        /// <summary>
        /// Gets or sets the id of the item.
        /// </summary>
        public String Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets the command that requests the item be maximized.
        /// </summary>
        public ICommand MaximizeCommand => new RelayCommand(this.MaximizeItem, () => !this.DisableMaximize);

        /// <summary>
        /// Gets the command that requests the item be minimized.
        /// </summary>
        public ICommand MinimizeCommand => new RelayCommand(this.MinimizeItem, () => !this.DisableMinimize);

        /// <summary>
        /// Gets the command that requests the item be restored from the minimized state.
        /// </summary>
        public ICommand RestoreCommand => new RelayCommand(this.RestoreItem);

        /// <summary>
        /// Gets the command that requests the item be shown.
        /// </summary>
        // CONSIDER: Does this really need to be separate from RestoreCommand?
        public ICommand ShowCommand => new RelayCommand(this.ShowItem);

        /// <summary>
        /// Gets a property dictionary usable for implementation specific purposes.
        /// </summary>
        public Dictionary<Object, Object> Tags { get; } = [];

        /// <summary>
        /// Invokes the <see cref="CloseRequested"/> event if <see cref="DisableClose"/> is <c>false</c>.
        /// </summary>
        private void CloseItem()
        {
            if (!this.DisableClose)
            {
                DockItemCloseRequestedEventArgs args = new(this);
                this.CloseRequested?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="FloatRequested"/> event.
        /// </summary>
        private void FloatItem()
        {
            DockItemFloatRequestedEventArgs args = new(this);
            this.FloatRequested?.Invoke(this, args);
        }

        /// <summary>
        /// Invokes the <see cref="HideRequested"/> event if <see cref="DisableHide"/> is <c>false</c>.
        /// </summary>
        private void HideItem()
        {
            if (!this.DisableHide)
            {
                DockItemHideRequestedEventArgs args = new(this);
                this.HideRequested?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="MaximizeRequested"/> event if <see cref="DisableMaximize"/> is <c>false</c>.
        /// </summary>
        private void MaximizeItem()
        {
            if (!this.DisableMaximize)
            {
                DockItemMaximizeRequestedEventArgs args = new(this);
                this.MaximizeRequested?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="MinimizeRequested"/> event if <see cref="DisableMinimize"/> is <c>false</c>.
        /// </summary>
        private void MinimizeItem()
        {
            if (!this.DisableMinimize)
            {
                DockItemMinimizeRequestedEventArgs args = new(this);
                this.MinimizeRequested?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Invokes the <see cref="RestoreRequested"/> event.
        /// </summary>
        private void RestoreItem()
        {
            DockItemRestoreRequestedEventArgs args = new(this);
            this.RestoreRequested?.Invoke(this, args);
        }

        /// <summary>
        /// Invokes the <see cref="ShowRequested"/> event.
        /// </summary>
        private void ShowItem()
        {
            DockItemShowRequestedEventArgs args = new(this);
            this.ShowRequested?.Invoke(this, args);
        }
    }
}
