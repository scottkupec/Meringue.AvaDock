// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Place holder for permissive handling of <see cref="DockItemViewModel.MaximizeRequested"/>.
    /// </summary>
    /// <remarks>Future looking event. Not yet implemented.</remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DockItemMaximizingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMaximizingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be maximized.</param>
        public DockItemMaximizingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
