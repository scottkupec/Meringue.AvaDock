// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents a serializable dock layout.
    /// </summary>
    // CONSIDER: How to support serializing DockItemViewModel.Context or a context factory to
    //           improve how apps can restore layouts.
    public sealed class DockLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayout"/> class.
        /// </summary>
        /// <param name="data">The underlying control data.</param>
        private DockLayout(DockControlData data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Gets the underlying control data for this layout.
        /// </summary>
        public DockControlData Data { get; }

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="items">The items to hide.</param>
        /// <returns>A list of <see cref="DockItemData"/>.</returns>
        public static List<DockItemData> Hidden(params DockItemData[] items) => [.. items];

        /// <summary>
        /// Creates a horizontal split node.
        /// </summary>
        /// <param name="children">The child nodes.</param>
        /// <returns>A new <see cref="DockSplitNodeData"/> instance.</returns>
        public static DockSplitNodeData Horizontal(params DockNodeData[] children) => Split(Orientation.Horizontal, children);

        /// <summary>
        /// Creates a dock item with the specified id and optional title and panel preference.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <param name="title">Optional title.</param>
        /// <param name="panelPreference">Optional panel preference.</param>
        /// <returns>A new <see cref="DockItemData"/> instance.</returns>
        public static DockItemData Item(String id, String? title = null, String? panelPreference = null)
        {
            return new DockItemData()
            {
                Id = id,
                Title = title ?? id,
                Panel = panelPreference,
            };
        }

        /// <summary>
        /// Creates a layout with optional primary workspace.
        /// </summary>
        /// <param name="primary">Optional primary workspace.</param>
        /// <returns>A new <see cref="DockLayout"/> instance.</returns>
        public static DockLayout Layout(DockWorkspaceData? primary = null)
        {
            DockControlData data = new()
            {
                PrimaryWorkspace = primary ?? Workspace(),
            };

            return new DockLayout(data);
        }

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="items">The items to minimize.</param>
        /// <returns>A list of <see cref="DockItemData"/>.</returns>
        public static List<DockItemData> Minimized(params DockItemData[] items) => [.. items];

        /// <summary>
        /// Creates a tab node with the specified id and items.
        /// </summary>
        /// <param name="id">The id of the tab node.</param>
        /// <param name="items">The items in the tab.</param>
        /// <returns>A new <see cref="DockTabNodeData"/> instance.</returns>
        public static DockTabNodeData Tab(String id, params DockItemData[] items)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(id, nameof(id));
            TargetFrameworkHelper.ThrowIfArgumentNull(items, nameof(items));

            return new DockTabNodeData
            {
                Id = id,
                Tabs = [.. items],
                SelectedId = items.Length > 0 ? items[0].Id : null,
            };
        }

        /// <summary>
        /// Creates a vertical split node.
        /// </summary>
        /// <param name="children">The child nodes.</param>
        /// <returns>A new <see cref="DockSplitNodeData"/> instance.</returns>
        public static DockSplitNodeData Vertical(params DockNodeData[] children) => Split(Orientation.Vertical, children);

        /// <summary>
        /// Creates a workspace with optional dock tree, minimized items, and hidden items.
        /// </summary>
        /// <param name="dockTree">Optional dock tree.</param>
        /// <param name="minimized">Optional minimized items.</param>
        /// <param name="hidden">Optional hidden items.</param>
        /// <returns>A new <see cref="DockWorkspaceData"/> instance.</returns>
        public static DockWorkspaceData Workspace(
            DockSplitNodeData? dockTree = null,
            List<DockItemData>? minimized = null,
            List<DockItemData>? hidden = null)
        {
            return new DockWorkspaceData
            {
                DockTree = dockTree ?? Split(Orientation.Horizontal),
                Minimized = minimized ?? [],
                Hidden = hidden ?? [],
            };
        }

        /// <summary>
        /// Merges this layout into the specified dock control manager.
        /// </summary>
        /// <typeparam name="T">The view model type to use for items.</typeparam>
        /// <param name="target">The target manager to merge into.</param>
        /// <param name="preserveContexts">Whether to preserve existing item contexts.</param>
        /// <returns>True if merge succeeded.</returns>
        public Boolean Merge<T>(DockControlManager target, Boolean preserveContexts = true)
            where T : DockItemViewModel, new()
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(target, nameof(target));

            if (this.Data is null)
            {
                return false;
            }

            DockControlManager built = DockSerializationConverter.BuildViewModel<T>(this.Data);

            IEnumerable<DockItemViewModel> allExistingItems = target.Items
                .Concat(target.MinimizedItems)
                .Concat(target.HiddenItems);

            foreach (DockItemViewModel runtimeItem in allExistingItems)
            {
                DockItemViewModel? serializedItem = built.FindItem(runtimeItem.Id);

                if (serializedItem is not null)
                {
                    if (preserveContexts)
                    {
                        serializedItem.Context = runtimeItem.Context;
                    }
                }
                else
                {
                    DockContext.SetItemState(runtimeItem, DockItemState.Hidden);
                    _ = built.AddItem(runtimeItem);
                }
            }

            built.PrimaryWorkspace.CommitChanges();

            foreach (DockWorkspaceManager secondaryWorkspace in built.SecondaryWorkspaces)
            {
                secondaryWorkspace.CommitChanges();
            }

            target.PrimaryWorkspace = built.PrimaryWorkspace;
            target.WindowManager = built.WindowManager;

            return true;
        }

        /// <summary>
        /// Creates a list of equal sizes for the specified count.
        /// </summary>
        /// <param name="count">The number of sizes to create.</param>
        /// <returns>A list of equal size values.</returns>
        private static List<Double> CreateEqualSizes(Int32 count)
        {
            if (count <= 0)
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

        /// <summary>
        /// Creates a split node with the specified orientation and children.
        /// </summary>
        /// <param name="orientation">The orientation of the split.</param>
        /// <param name="children">The child nodes.</param>
        /// <returns>A new <see cref="DockSplitNodeData"/> instance.</returns>
        private static DockSplitNodeData Split(Orientation orientation, params DockNodeData[] children)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(children, nameof(children));
            return new DockSplitNodeData
            {
                Orientation = orientation,
                Children = [.. children],
                Sizes = CreateEqualSizes(children.Length),
            };
        }
    }
}
