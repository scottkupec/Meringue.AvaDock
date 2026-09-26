// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being shown.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemShowingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemShowingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being shown.</param>
        public DockItemShowingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
