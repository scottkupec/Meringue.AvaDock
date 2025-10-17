// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.UnitTests
{
    /// <summary>
    /// Provides helpers for constructing declarative <see cref="DockNodeViewModel"/> trees used in unit tests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class DockTree
    {
        /// <summary>
        /// Recursively prints the structure of a <see cref="DockNodeViewModel"/> tree to the console.
        /// </summary>
        /// <param name="node">The root <see cref="DockNodeViewModel"/> to display.</param>
        /// <param name="indent">Indentation level for nested children.</param>
        /// <remarks>Not used in tests - present for use as a debug helper for adhoc testing.</remarks>
        public static void DisplayTree(DockNodeViewModel node, Int32 indent = 0)
        {
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

        /// <summary>
        /// Constructs a <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Horizontal"/> and the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">Child <see cref="DockNodeViewModel"/> to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Horizontal"/>.</returns>
        public static DockSplitNodeViewModel Horizontal(params DockNodeViewModel[] children)
            => Split(Orientation.Horizontal, children!);

        /// <summary>
        /// Constructs a <see cref="DockTabNodeViewModel"/> containing <see cref="DockItemViewModel"/>s
        /// with the specified item IDs.
        /// </summary>
        /// <param name="itemIds">Identifiers for the dock items to include as tabs.</param>
        /// <returns>A tab node with the specified items.</returns>
        public static DockTabNodeViewModel Tab(params String[] itemIds)
        {
            DockTabNodeViewModel tab = new();

            foreach (String itemId in itemIds)
            {
                DockItemViewModel item = new() { Id = itemId, Title = itemId };
                tab.AddTab(item);
            }

            return tab;
        }

        /// <summary>
        /// Constructs a <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Vertical"/> and the specified <paramref name="children"/>.
        /// </summary>
        /// <param name="children">Child <see cref="DockNodeViewModel"/> to include in the split.</param>
        /// <returns>A <see cref="DockSplitNodeViewModel"/> with <see cref="Orientation.Vertical"/>.</returns>
        public static DockSplitNodeViewModel Vertical(params DockNodeViewModel[] children)
            => Split(Orientation.Vertical, children!);

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
