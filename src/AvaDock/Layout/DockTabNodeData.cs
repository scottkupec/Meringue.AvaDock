// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockTabNodeViewModel"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Settable to simplify (de)serialization code.")]
    public sealed class DockTabNodeData : DockNodeData
    {
        /// <summary>
        /// Gets or sets the value of <see cref="DockTabNodeViewModel.Selected"/>.
        /// </summary>
        public String? SelectedId { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockTabNodeViewModel.Tabs"/>.
        /// </summary>
        public List<DockItemData> Tabs { get; set; } = [];
    }
}
