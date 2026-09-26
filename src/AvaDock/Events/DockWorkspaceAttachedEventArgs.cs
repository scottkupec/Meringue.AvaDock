// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockWorkspaceManager"/> is attached.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
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
