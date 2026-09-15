// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceAttached"/> event which is raised after a
    /// <see cref="DockWorkspaceManager"/> is attached.
    /// </summary>
    public class DockWorkspaceAttachedEventArgs : DockWorkspaceDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceAttachedEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was attached.</param>
        public DockWorkspaceAttachedEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
