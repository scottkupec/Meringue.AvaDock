// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock
{
    /// <summary>
    /// Provides helpers for constructing declarative <see cref="DockNodeViewModel"/> trees.
    /// </summary>
    public static class DockTree
    {
        /// <summary>
        /// Recursively prints the structure of a <see cref="DockNodeViewModel"/> tree to the console.
        /// </summary>
        /// <param name="node">The root <see cref="DockNodeViewModel"/> to display.</param>
        /// <param name="indent">Indentation level for nested children.</param>
        /// <remarks>Only present for use as a debug helper, not intended for normal application use.</remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void DisplayTree(DockNodeViewModel node, Int32 indent = 0)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(node, nameof(node));

            String indentStr = new(' ', indent * 2);

            switch (node)
            {
                case DockSplitNodeViewModel split:
                    Console.WriteLine($"{indentStr}DockSplitNodeViewModel ({split.Orientation})");
                    foreach (DockNodeViewModel child in split.Children)
                    {
                        DockTree.DisplayTree(child, indent + 1);
                    }

                    break;

                case DockTabNodeViewModel tabNode:
                    Console.WriteLine($"{indentStr}DockTabNodeViewModel");
                    foreach (DockItemViewModel item in tabNode.Tabs)
                    {
                        Console.WriteLine($"{indentStr}  DockItemViewModel (id={item.Id})");
                    }

                    break;

                default:
                    Console.WriteLine($"{indentStr}{node.GetType().Name}");
                    break;
            }
        }

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="workspaces">The workspaces for the control.</param>
        /// <returns>The new <see cref="DockControlManager"/>.</returns>
        public static DockControlManager Control(params DockWorkspaceManager[] workspaces)
        {
            if (workspaces is null || workspaces.Length == 0)
            {
                return new DockControlManager(
                    new DockWorkspaceManager(
                        new DockSplitNodeViewModel(Orientation.Horizontal)));
            }
            else if (workspaces.Length > 1)
            {
                // TODO: This needs size and position to be supported or we need to refactor how window
                //       creation for secondary workspaces is handled.  Not needed for initial version,
                //       but this should be on the radar for near-future support.
                throw new ArgumentOutOfRangeException(
                    nameof(workspaces),
                    "Secondary workspaces not yet supported for declarative construction.");
            }
            else
            {
                return new DockControlManager(workspaces[0]);
            }
        }

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="item">The item to hide.</param>
        /// <returns>A list of <see cref="DockItemViewModel"/>.</returns>
        public static List<DockItemViewModel> Hidden(DockItemViewModel item) => [item];

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="items">The items to hide.</param>
        /// <returns>A list of <see cref="DockItemViewModel"/>.</returns>
        public static List<DockItemViewModel> Hidden(DockItemViewModel[] items) => [.. items];

        /// <summary>
        /// Constructs a <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Horizontal"/> and the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">Child <see cref="DockNodeViewModel"/> to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Horizontal"/>.</returns>
        public static DockSplitNodeViewModel Horizontal(params DockNodeViewModel[] children) => DockTree.Split(Orientation.Horizontal, children ?? []);

        /// <summary>
        /// Creates a dock item with the specified id and optional title and panel preference.
        /// </summary>
        /// <param name="context">The context (display content) for the item.</param>
        /// <param name="id">The id of the item.</param>
        /// <param name="title">Optional title.</param>
        /// <returns>A new <see cref="DockItemViewModel"/> instance.</returns>
        public static DockItemViewModel Item(Object? context, String? id = null, String? title = null)
        {
            return new DockItemViewModel()
            {
                Id = id ?? Guid.NewGuid().ToString("N"),
                Title = title ?? "[Untitled]",
                Context = context,
            };
        }

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="item">The item to minimize.</param>
        /// <returns>A list of <see cref="DockItemViewModel"/>.</returns>
        public static List<DockItemViewModel> Minimized(DockItemViewModel item) => [item];

        /// <summary>Helper for more readable layout setups.</summary>
        /// <param name="items">The items to minimize.</param>
        /// <returns>A list of <see cref="DockItemViewModel"/>.</returns>
        public static List<DockItemViewModel> Minimized(params DockItemViewModel[] items) => [.. items];

        /// <summary>
        /// Constructs a <see cref="DockTabNodeViewModel"/> containing <see cref="DockItemViewModel"/>s
        /// with the specified item IDs.
        /// </summary>
        /// <param name="itemIds">Identifiers for the dock items to include as tabs.</param>
        /// <returns>A tab node with the specified items.</returns>
        public static DockTabNodeViewModel Tab(params String[] itemIds)
        {
            DockTabNodeViewModel tab = new();

            foreach (String itemId in itemIds ?? [])
            {
                DockItemViewModel item = new() { Id = itemId, Title = itemId };
                tab.AddTab(item);
            }

            return tab;
        }

        /// <summary>
        /// Constructs a <see cref="DockTabNodeViewModel"/> containing <see cref="DockItemViewModel"/>s
        /// with the specified item IDs.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/>s to add to the tab.</param>
        /// <returns>A tab node with the specified items.</returns>
        public static DockTabNodeViewModel Tab(DockItemViewModel item) => DockTree.Tab([item]);

        /// <summary>
        /// Constructs a <see cref="DockTabNodeViewModel"/> containing <see cref="DockItemViewModel"/>s
        /// with the specified item IDs.
        /// </summary>
        /// <param name="items">The stacked <see cref="DockItemViewModel"/>s to add to the tab.</param>
        /// <returns>A tab node with the specified items.</returns>
        public static DockTabNodeViewModel Tab(params DockItemViewModel[] items)
        {
            DockTabNodeViewModel tab = new();

            foreach (DockItemViewModel item in items ?? [])
            {
                tab.AddTab(item);
            }

            return tab;
        }

        /// <summary>
        /// Constructs a <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Vertical"/> and the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">Child <see cref="DockNodeViewModel"/> to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Vertical"/>.</returns>
        public static DockSplitNodeViewModel Vertical(params DockNodeViewModel[] children) => DockTree.Split(Orientation.Vertical, children ?? []);

        /// <summary>
        /// Creates a <see cref="DockWorkspaceManager"/> with optional dock tree, minimized items, and hidden items.
        /// </summary>
        /// <param name="dockTree">Optional dock tree.</param>
        /// <param name="minimized">Optional minimized items.</param>
        /// <param name="hidden">Optional hidden items.</param>
        /// <returns>A new <see cref="DockWorkspaceData"/> instance.</returns>
        public static DockWorkspaceManager Workspace(
            DockSplitNodeViewModel? dockTree = null,
            List<DockItemViewModel>? minimized = null,
            List<DockItemViewModel>? hidden = null)
        {
            DockWorkspaceManager workspace = new(dockTree ?? Split(Orientation.Horizontal, []));

            foreach (DockItemViewModel minimizedItem in minimized ?? [])
            {
                DockContext.SetItemState(minimizedItem, DockItemState.Minimized);
                _ = workspace.AddItem(minimizedItem);
            }

            foreach (DockItemViewModel hiddenItem in minimized ?? [])
            {
                DockContext.SetItemState(hiddenItem, DockItemState.Hidden);
                _ = workspace.AddItem(hiddenItem);
            }

            return workspace;
        }

        /// <summary>
        /// Internal helper to construct a <see cref="DockSplitNodeViewModel"/> with the given <see cref="Orientation"/> and <paramref name="children"/>.
        /// </summary>
        /// <param name="orientation">Split <see cref="Orientation"/>.</param>
        /// <param name="children">Child <see cref="DockNodeViewModel"/> to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeViewModel"/> with the specified <paramref name="orientation"/>.</returns>
        private static DockSplitNodeViewModel Split(Orientation orientation, DockNodeViewModel[] children)
        {
            DockSplitNodeViewModel split = new(orientation);
            foreach (DockNodeViewModel child in children)
            {
                split.AddChild(child);
            }

            return split;
        }
    }
}
