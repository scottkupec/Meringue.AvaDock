// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Base class for cancellable events related to <see cref="DockWorkspaceManager"/> changes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockWorkspaceDoingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDoingEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> associated with the event.</param>
        public DockWorkspaceDoingEventArgs(DockWorkspaceManager workspace)
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
