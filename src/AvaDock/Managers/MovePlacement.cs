// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Defines how a move operation should place the moved item in relation to the existing item.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public enum MovePlacement
    {
        /// <summary>
        /// The moved item should be placed on (stacked) the existing item.
        /// </summary>
        On = 0,

        /// <summary>
        /// The moved item should be placed after the existing item.
        /// </summary>
        After = 1,

        /// <summary>
        /// The moved item should be placed before the existing item.
        /// </summary>
        Before = 2,
    }
}
