// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being hidden.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemHidingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemHidingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being hidden.</param>
        public DockItemHidingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
