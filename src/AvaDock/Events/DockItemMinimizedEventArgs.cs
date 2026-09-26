// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is minimized.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemMinimizedEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMinimizedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was minimized.</param>
        public DockItemMinimizedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
