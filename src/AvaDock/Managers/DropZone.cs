// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Defines the zones where a item can be dropped when dragging
    /// to a <see cref="DockTabPanel"/>.
    /// </summary>
    public enum DropZone
    {
        /// <summary>
        /// No dock location is currently being hovered over.
        /// </summary>
        None,

        /// <summary>
        /// Split the target <see cref="DockTabPanel"/> horizontally adding the new item below
        /// current content.
        /// </summary>
        Bottom,

        /// <summary>
        /// Add the item as a new tab to the target <see cref="DockTabPanel"/>.
        /// </summary>
        Center,

        /// <summary>
        /// Split the target <see cref="DockTabPanel"/> vertically adding the new item to the
        /// left of the current content.
        /// </summary>
        Left,

        /// <summary>
        /// Split the target <see cref="DockTabPanel"/> vertically adding the new item to the
        /// right of the current content.
        /// </summary>
        Right,

        /// <summary>
        /// Split the target <see cref="DockTabPanel"/> horizontally adding the new item above
        /// current content.
        /// </summary>
        Top,
    }
}
