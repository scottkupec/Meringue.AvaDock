// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemMoving"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is moving.
    /// </summary>
    public class DockItemMovingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMovingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be closed.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved to.</param>
        public DockItemMovingEventArgs(DockItemViewModel item, DockTabNodeViewModel? fromNode, DockNodeViewModel? toNode)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
        }

        /// <summary>
        /// Gets the <see cref="DockTabNodeViewModel"/> that the item is being moved from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the <see cref="DockNodeViewModel"/> that the item is being moved to. If this is a <see cref="DockSplitNodeViewModel"/>,
        /// then a <see cref="DockTabNodeViewModel"/> will be created to hold the item.
        /// </summary>
        public DockNodeViewModel? ToNode { get; }
    }
}
