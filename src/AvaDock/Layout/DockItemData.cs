// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Text.Json.Serialization;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of a <see cref="DockItemViewModel"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "Inappropriate here.")]
    public sealed class DockItemData
    {
        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.Id"/>.
        /// </summary>
        public String Id { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.Title"/>.
        /// </summary>
        // Intentionally out of order for serialization.
        public String? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.DisableClose"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Boolean DisableClose { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.DisableHide"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Boolean DisableHide { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.DisableMaximize"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Boolean DisableMaximize { get; set; }

        /// <summary>
        /// Gets or sets the value of <see cref="DockItemViewModel.DisableMinimize"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Boolean DisableMinimize { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DockItemViewModel.Tags"/> entry used by <see cref="DockWorkspaceManager"/>.
        /// </summary>
        public String? Panel { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="DockItemViewModel.Tags"/> entry used by <see cref="DockControlManager"/>.
        /// </summary>
        public String? Workspace { get; set; }
    }
}
