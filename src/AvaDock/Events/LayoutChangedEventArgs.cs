// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.LayoutChanged"/> event.
    /// </summary>
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
