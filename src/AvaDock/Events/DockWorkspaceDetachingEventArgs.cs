// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceDetaching"/> event which is raised when a
    /// <see cref="DockWorkspaceManager"/> is being detached.
    /// </summary>
    public class DockWorkspaceDetachingEventArgs : DockWorkspaceDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDetachingEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was detached.</param>
        public DockWorkspaceDetachingEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
