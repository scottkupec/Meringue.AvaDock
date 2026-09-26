// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being closed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemClosingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemClosingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being closed.</param>
        public DockItemClosingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
