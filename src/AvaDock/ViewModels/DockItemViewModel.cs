// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Layout;
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
        /// Gets the close command.
        /// </summary>
        private readonly RelayCommand closeCommand;

        /// <summary>
        /// Gets the float command.
        /// </summary>
        private readonly RelayCommand floatCommand;

        /// <summary>
        /// Gets the hide command.
        /// </summary>
        private readonly RelayCommand hideCommand;

        /// <summary>
        /// Gets the make active command.
        /// </summary>
        private readonly RelayCommand makeActiveCommand;

        /// <summary>
        /// Gets the maximize command.
        /// </summary>
        private readonly RelayCommand maximizeCommand;

        /// <summary>
        /// Gets the minimize command.
        /// </summary>
        private readonly RelayCommand minimizeCommand;

        /// <summary>
        /// Gets the restore command.
        /// </summary>
        private readonly RelayCommand restoreCommand;

        /// <summary>
        /// Gets the show command.
        /// </summary>
        private readonly RelayCommand showCommand;

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
        /// Gets or sets a value indicating whether the item is currently active.
        /// </summary>
        [ObservableProperty]
        private Boolean active;

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being closed.
        /// </summary>
        [ObservableProperty]
        private Boolean disableClose;

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being hidden.
        /// </summary>
        [ObservableProperty]
        private Boolean disableHide;

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being maximized.
        /// </summary>
        [ObservableProperty]
        private Boolean disableMaximize;

        /// <summary>
        /// Gets or sets a value indicating whether the item blocks being minimized.
        /// </summary>
        [ObservableProperty]
        private Boolean disableMinimize;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemViewModel"/> class.
        /// </summary>
        public DockItemViewModel()
        {
            this.closeCommand = new RelayCommand(this.CloseItem, () => !this.DisableClose);
            this.floatCommand = new RelayCommand(this.FloatItem);
            this.hideCommand = new RelayCommand(this.HideItem, () => !this.DisableHide);
            this.makeActiveCommand = new RelayCommand(this.MakeActive);
            this.maximizeCommand = new RelayCommand(this.MaximizeItem, () => !this.DisableMaximize);
            this.minimizeCommand = new RelayCommand(this.MinimizeItem, () => !this.DisableMinimize);
            this.restoreCommand = new RelayCommand(this.RestoreItem);
            this.showCommand = new RelayCommand(this.ShowItem);
        }

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
        /// Occurs when a request is made to make this item the active item.
        /// </summary>
        public event EventHandler<DockItemActivateRequestedEventArgs>? MakeActiveRequested;

        /// <summary>
        /// Occurs when a request is made to maximize this item.
        /// </summary>
        public event EventHandler<DockItemMaximizeRequestedEventArgs>? MaximizeRequested;

        /// <summary>
        /// Occurs when a request is made to minimize this item.
        /// </summary>
        public event EventHandler<DockItemMinimizeRequestedEventArgs>? MinimizeRequested;

        /// <summary>
        /// Occurs when a request is made to move this item.
        /// </summary>
        public event EventHandler<DockItemMoveRequestedEventArgs>? MoveRequested;

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
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="CloseRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand CloseCommand => this.closeCommand;

        /// <summary>
        /// Gets the command that requests the item be floated.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Programmatic use should prefer <see cref="RequestFloat"/>.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand FloatCommand => this.floatCommand;

        /// <summary>
        /// Gets the command that requests the item be hidden.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="HideRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand HideCommand => this.hideCommand;

        /// <summary>
        /// Gets or sets the id of the item.
        /// </summary>
        public String Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets the command that requests the item be makde the active item.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="MakeActiveRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand MakeActiveCommand => this.makeActiveCommand;

        /// <summary>
        /// Gets the command that requests the item be maximized.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="MaximizeRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand MaximizeCommand => this.maximizeCommand;

        /// <summary>
        /// Gets the command that requests the item be minimized.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="MinimizeRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand MinimizeCommand => this.minimizeCommand;

        /// <summary>
        /// Gets the command that requests the item be restored from the minimized state.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="RestoreRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand RestoreCommand => this.restoreCommand;

        /// <summary>
        /// Gets the command that requests the item be shown.
        /// </summary>
        /// <remarks>
        /// UI-only binding surface. Use the <see cref="ShowRequested"/> event for programmatic requests.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICommand ShowCommand => this.showCommand;

        /// <summary>
        /// Gets a property dictionary usable for implementation specific purposes.
        /// </summary>
        public Dictionary<Object, Object> Tags { get; } = [];

        /// <summary>
        /// Requests that the current item be floated.
        /// </summary>
        /// <param name="screenLocation">The top-left position of the floated window in screen coordinates. Optional.</param>
        /// <param name="windowSize">The size of the floating window. Optional.</param>
        [EditorBrowsable(EditorBrowsableState.Never)] // Intended for UI use only.
        public void RequestFloat(PixelPoint? screenLocation, Size? windowSize)
        {
            DockItemFloatRequestedEventArgs args = new(this, screenLocation, windowSize);
            this.FloatRequested?.Invoke(this, args);
        }

        /// <summary>
        /// Requests a move to be performed on the current instance.
        /// </summary>
        /// <param name="sourceNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved from.</param>
        /// <param name="targetNode">The <see cref="DockTabNodeViewModel"/> representing the target of the move.</param>
        /// <param name="placement">The <see cref="MovePlacement"/> indicating how the item should be placed relative to the <paramref name="targetNode"/>.</param>
        /// <param name="requiredOrientation">
        /// The <see cref="Orientation"/> to be used when placing the node if <paramref name="placement"/> is <see cref="MovePlacement.Before"/> or
        /// <see cref="MovePlacement.After"/>. Optional and ignored for <see cref="MovePlacement.On"/>.
        /// </param>
        /// <param name="insertionIndex">The zero-based index at which the item should be inserted within the target node, if applicable.</param>
        [EditorBrowsable(EditorBrowsableState.Never)] // Intended for UI use only.
        public void RequestMove(DockTabNodeViewModel sourceNode, DockTabNodeViewModel targetNode, MovePlacement placement, Orientation? requiredOrientation, Int32? insertionIndex = null)
        {
            DockItemMoveRequestedEventArgs moveEvent = new(
                this,
                sourceNode,
                targetNode,
                placement,
                requiredOrientation,
                insertionIndex);
            this.MoveRequested?.Invoke(this, moveEvent);
        }

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
            DockItemFloatRequestedEventArgs args = new(this, null, null);
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
        /// Invokes the <see cref="MakeActiveRequested"/> event.
        /// </summary>
        private void MakeActive()
        {
            DockItemActivateRequestedEventArgs args = new(this);
            this.MakeActiveRequested?.Invoke(this, args);
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

        /// <inheritdoc/>
        partial void OnDisableCloseChanged(bool value)
        {
            closeCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableCloseChanged(bool oldValue, bool newValue)
        {
            closeCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableHideChanged(bool value)
        {
            hideCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableHideChanged(bool oldValue, bool newValue)
        {
            hideCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableMaximizeChanged(bool value)
        {
            maximizeCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableMaximizeChanged(bool oldValue, bool newValue)
        {
            maximizeCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableMinimizeChanged(bool value)
        {
            minimizeCommand.NotifyCanExecuteChanged();
        }

        /// <inheritdoc/>
        partial void OnDisableMinimizeChanged(bool oldValue, bool newValue)
        {
            minimizeCommand.NotifyCanExecuteChanged();
        }
    }
}
