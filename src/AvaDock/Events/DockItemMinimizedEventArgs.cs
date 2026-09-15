// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockWorkspaceManager.ItemMinimized"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is minimized.
    /// </summary>
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
