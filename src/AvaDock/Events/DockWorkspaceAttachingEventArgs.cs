// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.WorkspaceAttaching"/> event which is raised when a
    /// <see cref="DockWorkspaceManager"/> is being attached.
    /// </summary>
    public class DockWorkspaceAttachingEventArgs : DockWorkspaceDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceAttachingEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was attached.</param>
        public DockWorkspaceAttachingEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
