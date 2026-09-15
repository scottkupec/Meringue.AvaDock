// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for events that are raised by <see cref="DockItemViewModel"/>s to request an action.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemRequestEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting the action.</param>
        public DockItemRequestEventArgs(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            this.Item = item;
        }

        /// <summary>
        /// Gets the <see cref="DockItemViewModel"/> that is requesting the action.
        /// </summary>
        public DockItemViewModel Item { get; }
    }
}
