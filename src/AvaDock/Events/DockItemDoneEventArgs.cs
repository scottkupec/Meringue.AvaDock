// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for completed events related to <see cref="DockItemViewModel"/> state changes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemDoneEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemDoneEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was acted upon.</param>
        public DockItemDoneEventArgs(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            this.Item = item;
        }

        /// <summary>
        /// Gets the item that is associated with the event.
        /// </summary>
        public DockItemViewModel Item { get; }
    }
}
