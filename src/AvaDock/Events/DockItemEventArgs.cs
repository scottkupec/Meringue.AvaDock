// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for events that can be raised by <see cref="DockItemViewModel"/>s.
    /// </summary>
    public class DockItemEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        public DockItemEventArgs(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            this.Item = item;
        }

        /// <summary>
        /// Gets the item that is associated with the request.
        /// </summary>
        public DockItemViewModel Item { get; }
    }
}
