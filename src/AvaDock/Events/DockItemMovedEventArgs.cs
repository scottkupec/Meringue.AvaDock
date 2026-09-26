// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.ComponentModel;
using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Raised when a <see cref="DockItemViewModel"/> is moved.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemMovedEventArgs : DockItemDoneEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemMovedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was moved.</param>
        /// <param name="fromNode">The <see cref="DockTabNodeViewModel"/> that the item was moved from.</param>
        /// <param name="toNode">The <see cref="DockTabNodeViewModel"/> that the item was moved to.</param>
        /// <param name="placement">How the item was placed relative to the target node.</param>
        /// <param name="orientation">The orientation used for the placing the <see cref="DockItemViewModel"/>.</param>
        /// <param name="insertionIndex">The index at which the item was inserted into the target node, if applicable.</param>
        public DockItemMovedEventArgs(
            DockItemViewModel item,
            DockTabNodeViewModel? fromNode,
            DockTabNodeViewModel? toNode,
            MovePlacement placement,
            Orientation? orientation = null,
            Int32? insertionIndex = null)
            : base(item)
        {
            this.FromNode = fromNode;
            this.ToNode = toNode;
            this.Placement = placement;
            this.RequiredOrientation = orientation;
            this.InsertionIndex = insertionIndex;
        }

        /// <summary>
        /// Gets the <see cref="DockTabNodeViewModel"/> that the item was moved from.
        /// </summary>
        public DockTabNodeViewModel? FromNode { get; }

        /// <summary>
        /// Gets the index at which the item was inserted into the target node, or <c>null</c> if not specified.
        /// </summary>
        public Int32? InsertionIndex { get; }

        /// <summary>
        /// Gets the <see cref="MovePlacement"/> for how the item was placed in <see cref="ToNode"/>.
        /// </summary>
        public MovePlacement Placement { get; }

        /// <summary>
        /// Gets the <see cref="Orientation"/> used for placing the item, or <c>null</c> if orientation doesn't apply.
        /// </summary>
        public Orientation? RequiredOrientation { get; }

        /// <summary>
        /// Gets the <see cref="DockNodeViewModel"/> that the item was moved to.
        /// </summary>
        public DockTabNodeViewModel? ToNode { get; }
    }
}
