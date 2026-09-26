// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockWorkspaceManager"/> is being attached.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockWorkspaceAttachingEventArgs : DockWorkspaceDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceAttachingEventArgs"/> class.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that is being attached.</param>
        public DockWorkspaceAttachingEventArgs(DockWorkspaceManager workspace)
            : base(workspace)
        {
        }
    }
}
