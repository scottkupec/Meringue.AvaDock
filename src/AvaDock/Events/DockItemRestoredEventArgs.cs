// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is restored.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemRestoredEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemRestoredEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was restored.</param>
        public DockItemRestoredEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
