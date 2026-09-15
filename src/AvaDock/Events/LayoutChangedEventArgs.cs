// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Enumerates the types of layout changes that can occur.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Closely related classes.")]
    public enum LayoutChangeType
    {
        /// <summary>An item was added to the layout.</summary>
        ItemAdded = 0,

        /// <summary>An item was removed from the layout.</summary>
        ItemRemoved = 1,

        /// <summary>An item was moved within the layout.</summary>
        ItemMoved = 2,

        /// <summary>A workspace was attached.</summary>
        WorkspaceAttached = 3,

        /// <summary>A workspace was detached.</summary>
        WorkspaceDetached = 4,

        /// <summary>The layout state cannot be determined or changed too much.</summary>
        Undetermined = 5,
    }

    /// <summary>
    /// Provides data for the <see cref="DockControlManager.LayoutChanged"/> event.
    /// </summary>
    public class LayoutChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutChangedEventArgs"/> class.
        /// </summary>
        /// <param name="affectedItems">The affected <see cref="DockItemViewModel"/>s that changed.</param>
        /// <param name="affectedWorkspaces">The affected <see cref="DockWorkspaceManager"/>s that changed.</param>
        /// <param name="changeType">The type of layout change that occurred.</param>
        public LayoutChangedEventArgs(
            IReadOnlyList<DockItemViewModel> affectedItems,
            IReadOnlyList<DockWorkspaceManager> affectedWorkspaces,
            LayoutChangeType changeType)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(affectedItems);
            TargetFrameworkHelper.ThrowIfArgumentNull(affectedWorkspaces);
            TargetFrameworkHelper.ThrowIfArgumentNull(changeType);

            this.AffectedItems = affectedItems;
            this.AffectedWorkspaces = affectedWorkspaces;
            this.ChangeType = changeType;
            this.Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets the affected items that were changed.
        /// </summary>
        public IReadOnlyList<DockItemViewModel> AffectedItems { get; }

        /// <summary>
        /// Gets the affected workspaces that were changed.
        /// </summary>
        public IReadOnlyList<DockWorkspaceManager> AffectedWorkspaces { get; }

        /// <summary>
        /// Gets the type of layout change that occurred.
        /// </summary>
        public LayoutChangeType ChangeType { get; }

        /// <summary>
        /// Gets the timestamp when the layout change was detected.
        /// </summary>
        public DateTime Timestamp { get; }
    }
}
