// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for cancellable events related to <see cref="DockItemViewModel"/> state changes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemDoingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemDoingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that will be acted upon.</param>
        public DockItemDoingEventArgs(DockItemViewModel item)
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
