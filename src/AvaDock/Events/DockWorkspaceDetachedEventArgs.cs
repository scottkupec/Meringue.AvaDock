// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockWorkspaceManager"/> is detached.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
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
