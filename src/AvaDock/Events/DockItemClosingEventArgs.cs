// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemClosing"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is being closed.
    /// </summary>
    public class DockItemClosingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemClosingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being closed.</param>
        public DockItemClosingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
