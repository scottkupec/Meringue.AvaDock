// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for completed events related to <see cref="DockWorkspaceManager"/> state changes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockWorkspaceDoneEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDoneEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> associated with the event.</param>
        public DockWorkspaceDoneEventArgs(DockWorkspaceManager workspace)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);
            this.Workspace = workspace;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> associated with the event.
        /// </summary>
        public DockWorkspaceManager Workspace { get; }
    }
}
