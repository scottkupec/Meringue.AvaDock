// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.MaximizeRequested"/> event.
    /// </summary>
    public class DockItemMaximizeRequestedEventArgs : DockItemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMaximizeRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemMaximizeRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
