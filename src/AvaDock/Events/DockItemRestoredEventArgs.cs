// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemRestored"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is restored.
    /// </summary>
    public class DockItemRestoredEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemRestoredEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was restored.</param>
        public DockItemRestoredEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
