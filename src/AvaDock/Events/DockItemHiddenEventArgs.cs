// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemHidden"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is hidden.
    /// </summary>
    public class DockItemHiddenEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemHiddenEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was hidden.</param>
        public DockItemHiddenEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
