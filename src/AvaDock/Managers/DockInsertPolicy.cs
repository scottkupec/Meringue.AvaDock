// Copyright (C) Scott Kupec. All rights reserved.

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Defines the options for how non-existing items should be inserted into a <see cref="DockLayoutManager"/>.
    /// </summary>
    public enum DockInsertPolicy
    {
        /// <summary>
        /// Use the default insert policy.
        /// </summary>
        Default,

        /// <summary>
        /// If parent is not found, the new item is added at the beginning of the root container.
        /// </summary>
        CreateFirst = Default,

        /// <summary>
        /// If parent is not found, the new item is added at the end of the root container.
        /// </summary>
        CreateLast,

        /// <summary>
        /// If parent is not found, the new item is added to a new floating workspace.
        /// </summary>
        CreateFloating,

        /// <summary>
        /// If parent is not found, throw an exception and do not attach.
        /// </summary>
        Error,
    }
}
