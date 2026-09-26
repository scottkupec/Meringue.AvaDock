// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is closed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemClosedEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemClosedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was closed.</param>
        public DockItemClosedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
