// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Layout;
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
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being moved.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved to.</param>
        /// <param name="placement">How the item should be placed relative to the target node.</param>
        /// <param name="orientation">The orientation required for placing the <see cref="DockItemViewModel"/>.</param>
        public DockItemMovingEventArgs(
            DockItemViewModel item,
            DockTabNodeViewModel? fromNode,
            DockTabNodeViewModel? toNode,
            MovePlacement placement,
            Orientation? orientation = null)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
            this.Placement = placement;
            this.RequiredOrientation = orientation;
        }

        /// <summary>
        /// Gets the <see cref="DockTabNodeViewModel"/> that the item is being moved from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the <see cref="MovePlacement"/> for how the item should be placed in <see cref="ToNode"/>.
        /// </summary>
        public MovePlacement Placement { get; }

        /// <summary>
        /// Gets the <see cref="Avalonia.Layout.Orientation"/> required for placing the item when <see cref="Placement"/>
        /// is <see cref="MovePlacement.After"/> or <see cref="MovePlacement.Before"/>; otherwise null.
        /// </summary>
        public Orientation? RequiredOrientation { get; }

        /// <summary>
        /// Gets the <see cref="DockNodeViewModel"/> that the item is being moved to. If this is a <see cref="DockSplitNodeViewModel"/>,
        /// then a <see cref="DockTabNodeViewModel"/> will be created to hold the item.
        /// </summary>
        public DockTabNodeViewModel? ToNode { get; }
    }
}
