// Copyright (C) Scott Kupec. All rights reserved.

namespace Meringue.AvaDock.ViewModels
{
    /// <summary>
    /// Stores the display state of the control associated with a <see cref="DockItemViewModel"/>.
    /// </summary>
    public enum DockItemState
    {
        /// <summary>
        /// The <see cref="DockItemViewModel"/> is currently in the normal state.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The <see cref="DockItemViewModel"/> is currently minimized.
        /// </summary>
        Minimized = 1,

        /// <summary>
        /// The <see cref="DockItemViewModel"/> is currently maximized.
        /// </summary>
        Maximized = 2,

        /// <summary>
        /// The <see cref="DockItemViewModel"/> is currently hidden.
        /// </summary>
        Hidden = 3,
    }
}
