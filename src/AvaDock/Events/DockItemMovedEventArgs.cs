// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemMoved"/> event which is raised after a
    /// <see cref="DockItemViewModel"/> is moved.
    /// </summary>
    public class DockItemMovedEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMovedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was moved.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item was moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item was moved to.</param>
        public DockItemMovedEventArgs(DockItemViewModel item, DockTabNodeViewModel? fromNode, DockNodeViewModel? toNode)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
        }

        /// <summary>
        /// Gets the <see cref="DockTabNodeViewModel"/> that the item was moved from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the <see cref="DockNodeViewModel"/> that the item was moved to. If this is a <see cref="DockSplitNodeViewModel"/>,
        /// then a <see cref="DockTabNodeViewModel"/> was created to hold the item.
        /// </summary>
        public DockNodeViewModel? ToNode { get; }
    }
}
