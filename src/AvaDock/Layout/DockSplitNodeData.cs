// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockSplitNodeViewModel"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Settable to simplify (de)serialization code.")]
    public sealed class DockSplitNodeData : DockNodeData
    {
        /// <summary>
        /// Gets or sets the value of <see cref="DockSplitNodeViewModel.Children"/>.
        /// </summary>
        public List<DockNodeData> Children { get; set; } = [];

        /// <summary>
        /// Gets or sets the value of <see cref="DockSplitNodeViewModel.Orientation"/>.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockSplitNodeViewModel.Sizes"/>.
        /// </summary>
        public List<Double> Sizes { get; set; } = [];
    }
}
