// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemShowing"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is being shown.
    /// </summary>
    public class DockItemShowingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemShowingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being shown.</param>
        public DockItemShowingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
