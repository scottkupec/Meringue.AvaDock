// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockControlManager"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Settable to simplify (de)serialization code.")]
    public sealed class DockControlData
    {
        /// <summary>
        /// Gets or sets the major version of the layout format.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="DockSerializationConverter"/> to determine layout compatibility.
        /// </remarks>
        public Int32 Major { get; set; } = 1;

        /// <summary>
        /// Gets or sets the minor version of the layout format.
        /// </summary>
        /// <remarks>
        /// Reserved for future use by <see cref="DockSerializationConverter"/> to determine layout compatibility.
        /// </remarks>
        public Int32 Minor { get; set; } = 0;

        /// <summary>
        /// Gets or sets the patch version of the layout format.
        /// </summary>
        /// <remarks>
        /// Reserved for future use by <see cref="DockSerializationConverter"/> to determine layout compatibility.
        /// </remarks>
        public Int32 Patch { get; set; } = 0;

        /// <summary>
        /// Gets or sets the value of <see cref="DockControlManager.HiddenItems"/>.
        /// </summary>
        public List<DockItemData>? Hidden { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockControlManager.PrimaryWorkspace"/>.
        /// </summary>
        public DockWorkspaceData? PrimaryWorkspace { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Window"/>s needed to restore <see cref="DockControlManager.SecondaryWorkspaces"/>.
        /// </summary>
        public List<DockWindowData>? SecondaryWorkspaces { get; set; }
    }
}
