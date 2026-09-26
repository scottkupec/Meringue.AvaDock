// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockWorkspaceManager"/> is being detached.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockWorkspaceDetachingEventArgs : DockWorkspaceDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceDetachingEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that is being detached.</param>
        public DockWorkspaceDetachingEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
