// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockWorkspaceManager.ItemRestoring"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is being restored.
    /// </summary>
    public class DockItemRestoringEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemRestoringEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being restored.</param>
        public DockItemRestoringEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
