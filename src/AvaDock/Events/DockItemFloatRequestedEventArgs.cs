// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.FloatRequested"/> event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemFloatRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemFloatRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be floated.</param>
        public DockItemFloatRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
