// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemHiding"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is being hidden.
    /// </summary>
    public class DockItemHidingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemHidingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being hidden.</param>
        public DockItemHidingEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
