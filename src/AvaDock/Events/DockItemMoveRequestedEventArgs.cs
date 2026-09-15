// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockControlManager.ItemMoving"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> is moving.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemMoveRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMoveRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be moved.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved to.</param>
        public DockItemMoveRequestedEventArgs(DockItemViewModel item, DockTabNodeViewModel? fromNode, DockNodeViewModel? toNode)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
        }

        /// <summary>
        /// Gets the source <see cref="DockTabNodeViewModel"/> that the item is being moved from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the destination <see cref="DockNodeViewModel"/> that the item is being moved to. If this is a <see cref="DockSplitNodeViewModel"/>,
        /// then a <see cref="DockTabNodeViewModel"/> will be created to hold the item.
        /// </summary>
        public DockNodeViewModel? ToNode { get; }
    }
}
