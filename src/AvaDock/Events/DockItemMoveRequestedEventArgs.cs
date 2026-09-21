// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Avalonia.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.MoveRequested"/> event which is raised when a
    /// <see cref="DockItemViewModel"/> wants to be moved.
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
        /// <param name="placement">How the item should be placed relative to the target node.</param>
        /// <param name="orientation">The orientation required for the placing the <see cref="DockItemViewModel"/>.</param>
        public DockItemMoveRequestedEventArgs(
            DockItemViewModel item,
            DockTabNodeViewModel? fromNode,
            DockTabNodeViewModel toNode,
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
        /// Gets the <see cref="Orientation"/> required for placing the item when <see cref="Placement"/>
        /// is <see cref="MovePlacement.After"/> or <see cref="MovePlacement.Before"/>; otherwise null.
        /// </summary>
        public Orientation? RequiredOrientation { get; }

        /// <summary>
        /// Gets the destination <see cref="DockTabNodeViewModel"/> that the item is being moved to.
        /// </summary>
        public DockTabNodeViewModel ToNode { get; }
    }
}
