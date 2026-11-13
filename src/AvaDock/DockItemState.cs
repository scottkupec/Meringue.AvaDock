// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock
{
    /// <summary>
    /// Stores the display state of the control associated with a <see cref="DockItemViewModel"/>.
    /// </summary>
    /// <remarks>
    /// This is a flags enum so the <see cref="Hidden"/> state can be combined with the
    /// <see cref="Minimized"/> state. Hidden and minimized are independent: an item can be
    /// hidden while minimized, and shown later into the minimized bar rather than a tab.
    /// </remarks>
    [Flags]
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
        /// <remarks>Not currently implemented.</remarks>
        Maximized = 2,

        /// <summary>
        /// The <see cref="DockItemViewModel"/> is currently hidden.
        /// </summary>
        Hidden = 4,
    }
}
