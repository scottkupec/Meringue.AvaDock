// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is hidden.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemHiddenEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemHiddenEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was hidden.</param>
        public DockItemHiddenEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
