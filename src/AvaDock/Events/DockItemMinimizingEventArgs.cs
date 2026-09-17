// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemMinimizing"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is being minimized.
    /// </summary>
    public class DockItemMinimizingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMinimizingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being minimized.</param>
        public DockItemMinimizingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
