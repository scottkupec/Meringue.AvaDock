// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.MinimizeRequested"/> event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemMinimizeRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMinimizeRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be minimized.</param>
        public DockItemMinimizeRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
