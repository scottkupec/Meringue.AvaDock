// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised for any change that impacts the <see cref="DockLayout"/> of a control managed by a <see cref="DockControlManager"/>.
    /// </summary>
    /// <remarks>
    /// This is an aggregate event for any change.  To listen for specific changes, see the event types related to those changes:
    /// <list type="bullet">
    ///   <item><see cref="DockItemClosedEventArgs"/></item>
    ///   <item><see cref="DockItemClosingEventArgs"/></item>
    ///   <item><see cref="DockItemHiddenEventArgs"/></item>
    ///   <item><see cref="DockItemHidingEventArgs"/></item>
    ///   <item><see cref="DockItemMinimizedEventArgs"/></item>
    ///   <item><see cref="DockItemMinimizingEventArgs"/></item>
    ///   <item><see cref="DockItemMovedEventArgs"/></item>
    ///   <item><see cref="DockItemMovingEventArgs"/></item>
    ///   <item><see cref="DockItemRestoredEventArgs"/></item>
    ///   <item><see cref="DockItemRestoringEventArgs"/></item>
    ///   <item><see cref="DockItemShowingEventArgs"/></item>
    ///   <item><see cref="DockItemShownEventArgs"/></item>
    ///   <item><see cref="DockWorkspaceAttachedEventArgs"/></item>
    ///   <item><see cref="DockWorkspaceAttachingEventArgs"/></item>
    ///   <item><see cref="DockWorkspaceDetachedEventArgs"/></item>
    ///   <item><see cref="DockWorkspaceDetachingEventArgs"/></item>
    ///  </list>
    ///  This event may be seen in bursts when a user is defining a new layout.  If monitoring for purposes
    ///  of saving the layout to disk, consider buffering the event for a few seconds to reduce the number
    ///  of write operations needed.
    /// </remarks>
    public class LayoutChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutChangedEventArgs"/> class.
        /// </summary>
        public LayoutChangedEventArgs()
        {
            this.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the timestamp when the layout change was detected.
        /// </summary>
        public DateTime Timestamp { get; }
    }
}
