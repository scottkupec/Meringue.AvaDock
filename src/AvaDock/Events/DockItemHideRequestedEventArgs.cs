// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.HideRequested"/> event.
    /// </summary>
    public class DockItemHideRequestedEventArgs : DockItemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemHideRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemHideRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
