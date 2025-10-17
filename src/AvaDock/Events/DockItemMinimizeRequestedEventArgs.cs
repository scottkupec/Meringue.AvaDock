// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.MinimizeRequested"/> event.
    /// </summary>
    public class DockItemMinimizeRequestedEventArgs : DockItemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMinimizeRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemMinimizeRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
