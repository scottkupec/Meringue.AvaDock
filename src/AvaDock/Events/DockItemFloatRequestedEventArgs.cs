// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.FloatRequested"/> event.
    /// </summary>
    public class DockItemFloatRequestedEventArgs : DockItemEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemFloatRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemFloatRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
