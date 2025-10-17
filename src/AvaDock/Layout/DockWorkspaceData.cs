// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockWorkspaceManager"/>.
    /// </summary>
    public sealed class DockWorkspaceData
    {
        /// <summary>
        /// Gets or sets the value of <see cref="DockWorkspaceManager.Id"/>.
        /// </summary>
        public String? Id { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockWorkspaceManager.DockTree"/>.
        /// </summary>
        public DockSplitNodeData? DockTree { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockWorkspaceManager.MinimizedItems"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Settable to simplify (de)serialization code.")]
        public List<DockItemData>? Minimized { get; set; }
    }
}
