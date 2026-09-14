// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceDetached"/> event.
    /// </summary>
    public class DockWorkspaceDetachedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDetachedEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was detached.</param>
        public DockWorkspaceDetachedEventArgs(DockWorkspaceManager workspace)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);
            this.Workspace = workspace;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> that was detached.
        /// </summary>
        public DockWorkspaceManager Workspace { get; }
    }
}
