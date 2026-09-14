// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceAttached"/> event.
    /// </summary>
    public class DockWorkspaceAttachedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceAttachedEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was attached.</param>
        public DockWorkspaceAttachedEventArgs(DockWorkspaceManager workspace)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);
            this.Workspace = workspace;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> that was attached.
        /// </summary>
        public DockWorkspaceManager Workspace { get; }
    }
}
