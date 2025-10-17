// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

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
        /// <summary>Gets the <see cref="Managers.DropZone"/> for the current operation.</summary>
        public DropZone DropZone { get; init; }

        /// <summary>Gets the <see cref="DockSplitNodeViewModel.Orientation"/> necessary for the current operation.</summary>
        public Orientation? RequiredOrientation { get; init; }
    }
}
