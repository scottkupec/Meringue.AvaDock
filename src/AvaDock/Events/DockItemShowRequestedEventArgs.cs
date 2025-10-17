// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.ShowRequested"/> event.
    /// </summary>
    public class DockItemShowRequestedEventArgs : DockItemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemShowRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemShowRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
