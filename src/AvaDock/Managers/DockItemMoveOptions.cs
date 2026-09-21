// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Layout;

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Options to abstract away UI-specific concepts.
    /// </summary>
    // CONSIDER: This seems more useful when it was first added, but how useful is it really
    //           versus just passing the two parameters?
    // TODO: Task 179 to remove.
    internal record DockItemMoveOptions
    {
        /// <summary>Gets the <see cref="MovePlacement"/> for the current operation.</summary>
        public MovePlacement Placement { get; init; }

        /// <summary>Gets the <see cref="Avalonia.Layout.Orientation"/> necessary for the current operation.</summary>
        public Orientation? RequiredOrientation { get; init; }
    }
}
