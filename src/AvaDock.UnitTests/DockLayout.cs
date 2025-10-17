// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;

namespace Meringue.AvaDock.UnitTests
{
    /// <summary>
    /// Provides factory methods for constructing <see cref="DockControlData"/> layouts used in unit tests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class DockLayout
    {
        /// <summary>
        /// Creates a list of hidden <see cref="DockItemData"/> items.
        /// </summary>
        /// <param name="items">The <see cref="DockItemData"/>s to mark as hidden.</param>
        /// <returns>A list of hidden <see cref="DockItemData"/>s.</returns>
        public static List<DockItemData> Hidden(params DockItemData[] items)
            => [.. items];

        /// <summary>
        /// Creates a <see cref="Orientation.Horizontal"/> <see cref="DockSplitNodeData"/> with the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">The child <see cref="DockNodeData"/>s to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeData"/> with <see cref="Orientation.Horizontal"/>.</returns>
        public static DockSplitNodeData Horizontal(params DockNodeData[] children)
            => Split(Orientation.Horizontal, children);

        /// <summary>
        /// Constructs a <see cref="DockItemData"/> with the specified ID, title, and panel.
        /// </summary>
        /// <param name="id">Unique identifier for the item.</param>
        /// <param name="title">Optional display title. Defaults to <paramref name="id"/>.</param>
        /// <param name="panelPreference">Optional panel preference. Used for minimized and hidden items.</param>
        /// <returns>A new <see cref="DockItemData"/>.</returns>
        public static DockItemData Item(String id, String? title = null, String? panelPreference = null)
        {
            return new DockItemData
            {
                Id = id,
                Title = title ?? id,
                Panel = panelPreference,
            };
        }

        /// <summary>
        /// Constructs a <see cref="DockControlData"/> layout with optional primary workspace and hidden items.
        /// </summary>
        /// <param name="primary">The primary <see cref="DockWorkspaceData"/>. Defaults to an empty horizontal split.</param>
        /// <param name="hiddenItems">Optional list of hidden <see cref="DockItemData"/>s.</param>
        /// <returns>A new <see cref="DockControlData"/>.</returns>
        public static DockControlData Layout(
            DockWorkspaceData? primary = null,
            List<DockItemData>? hiddenItems = null)
        {
            return new DockControlData
            {
                PrimaryWorkspace = primary ?? DockLayout.Workspace(),
                Hidden = hiddenItems ?? [],
            };
        }

        /// <summary>
        /// Creates a list of minimized <see cref="DockItemData"/> items.
        /// </summary>
        /// <param name="items">The <see cref="DockItemData"/>s to mark as minimized.</param>
        /// <returns>A list of <see cref="DockItemData"/>s.</returns>
        public static List<DockItemData> Minimized(params DockItemData[] items)
            => [.. items];

        /// <summary>
        /// Constructs a <see cref="DockSplitNodeData"/> with the specified <paramref name="orientation"/> and <paramref name="children"/>.
        /// </summary>
        /// <param name="orientation">The split <see cref="Orientation"/>.</param>
        /// <param name="children">The <see cref="DockNodeData"/>s to include in the <see cref="DockSplitNodeData"/>.</param>
        /// <returns>A <see cref="DockSplitNodeData"/> node with equal size distribution.</returns>
        public static DockSplitNodeData Split(
            Orientation orientation,
            params DockNodeData[] children)
        {
            return new DockSplitNodeData
            {
                Orientation = orientation,
                Children = [.. children],
                Sizes = CreateEqualSizes(children.Length),
            };
        }

        /// <summary>
        /// Constructs a <see cref="DockTabNodeData"/> with the specified ID and tab items.
        /// </summary>
        /// <param name="id">Unique identifier for the <see cref="DockTabNodeData"/>.</param>
        /// <param name="items">The <see cref="DockItemData"/>s to include as tabs.</param>
        /// <returns>A <see cref="DockTabNodeData"/> with the first item selected by default.</returns>
        public static DockTabNodeData Tab(String id, params DockItemData[] items)
        {
            return new DockTabNodeData
            {
                Id = id,
                Tabs = [.. items],
                SelectedId = items.Length > 0 ? items[0].Id : null,
            };
        }

        /// <summary>
        /// Creates a vertical <see cref="DockSplitNodeData"/> with the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">The <see cref="DockNodeData"/>s to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeData"/> with <see cref="Orientation.Vertical"/>.</returns>
        public static DockSplitNodeData Vertical(params DockNodeData[] children)
            => Split(Orientation.Vertical, children);

        /// <summary>
        /// Constructs a <see cref="DockWorkspaceData"/> with optional <paramref name="dockTree"/> and <paramref name="minimizedItems"/>.
        /// </summary>
        /// <param name="dockTree">The root <see cref="DockItemData"/>. Defaults to an empty <see cref="DockItemData"/> with <see cref="Orientation.Horizontal"/>.</param>
        /// <param name="minimizedItems">Optional list of minimized <see cref="DockItemData"/>s.</param>
        /// <returns>A new workspace layout.</returns>
        public static DockWorkspaceData Workspace(
            DockSplitNodeData? dockTree = null,
            List<DockItemData>? minimizedItems = null)
        {
            return new DockWorkspaceData
            {
                DockTree = dockTree ?? Split(Orientation.Horizontal),
                Minimized = minimizedItems ?? [],
            };
        }

        /// <summary>
        /// Creates a list of equal size weights for a <see cref="DockSplitNodeData"/>.
        /// </summary>
        /// <param name="count">The number of children in the split.</param>
        /// <returns>A list of equal size ratios summing to 1.0.</returns>
        private static List<Double> CreateEqualSizes(Int32 count)
        {
            if (count == 0)
            {
                return [];
            }

            Double size = 1.0 / count;
            List<Double> sizes = [];
            for (Int32 i = 0; i < count; i++)
            {
                sizes.Add(size);
            }

            return sizes;
        }
    }
}
