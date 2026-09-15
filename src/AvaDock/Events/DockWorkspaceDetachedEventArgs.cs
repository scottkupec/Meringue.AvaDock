// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceDetached"/> event which is raised after a
    /// <see cref="DockWorkspaceManager"/> is detached.
    /// </summary>
    public class DockWorkspaceDetachedEventArgs : DockWorkspaceDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDetachedEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was detached.</param>
        public DockWorkspaceDetachedEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
