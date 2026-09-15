// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemShown"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is shown.
    /// </summary>
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
