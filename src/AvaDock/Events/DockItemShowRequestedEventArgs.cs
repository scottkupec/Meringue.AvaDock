// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.ShowRequested"/> event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemShowRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemShowRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be shown.</param>
        public DockItemShowRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
