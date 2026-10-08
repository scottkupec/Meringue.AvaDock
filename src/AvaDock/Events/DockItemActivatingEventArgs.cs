// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being activated.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemActivatingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemActivatingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being activated.</param>
        public DockItemActivatingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
