// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being maximized.
    /// </summary>
    /// <remarks>Future looking event. Not yet implemented.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DockItemMaximizingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMaximizingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being maximized.</param>
        public DockItemMaximizingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
