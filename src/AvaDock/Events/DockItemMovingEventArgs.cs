// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.ComponentModel;
using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is being moved.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemMovingEventArgs : DockItemDoingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMovingEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being moved.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item is being moved to.</param>
        /// <param name="placement">How the item should be placed.</param>
        /// <param name="orientation">The orientation required for placing the <see cref="DockItemViewModel"/>.</param>
        /// <param name="insertionIndex">The index at which the item should be inserted into the target node, if applicable.</param>
        public DockItemMovingEventArgs(
            DockItemViewModel item,
            DockTabNodeViewModel? fromNode,
            DockTabNodeViewModel? toNode,
            MovePlacement placement,
            Orientation? orientation,
            Int32? insertionIndex)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
            this.Placement = placement;
            this.RequiredOrientation = orientation;
            this.InsertionIndex = insertionIndex;
        }

        /// <summary>
        /// Gets the <see cref="DockTabNodeViewModel"/> that the item is moving from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the index at which the item will be inserted into the target node, or <c>null</c> if not specified.
        /// </summary>
        public Int32? InsertionIndex { get; }

        /// <summary>
        /// Gets the <see cref="MovePlacement"/> for how the item will be placed in <see cref="ToNode"/>.
        /// </summary>
        public MovePlacement Placement { get; }

        /// <summary>
        /// Gets the <see cref="Orientation"/> to be used for placing the item, or <c>null</c> if orientation doesn't apply.
        /// </summary>
        public Orientation? RequiredOrientation { get; }

        /// <summary>
        /// Gets the <see cref="DockNodeViewModel"/> that the item is moving to.
        /// </summary>
        public DockTabNodeViewModel? ToNode { get; }
    }
}
