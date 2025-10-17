// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Text.Json.Serialization;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockNodeViewModel"/>.
    /// </summary>
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(DockSplitNodeData), "split")]
    [JsonDerivedType(typeof(DockTabNodeData), "tab")]
    public abstract class DockNodeData
    {
        /// <summary>
        /// Gets or sets value of <see cref="DockNodeViewModel.Id"/>.
        /// </summary>
        [JsonPropertyOrder(Int32.MinValue)] // Ensures Id comes first in serialized output.
        public String? Id { get; set; }
    }
}
