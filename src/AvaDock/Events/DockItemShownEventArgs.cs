// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is shown (restored from the hidden state).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemShownEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemShownEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was shown.</param>
        public DockItemShownEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
