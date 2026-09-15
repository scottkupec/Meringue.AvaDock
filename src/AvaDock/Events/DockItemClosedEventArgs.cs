// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemClosed"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is closed.
    /// </summary>
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
