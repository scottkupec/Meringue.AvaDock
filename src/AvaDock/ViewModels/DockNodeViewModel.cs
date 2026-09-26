// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.ViewModels
{
    /// <summary>A common base for nodes that can participate directly in a <see cref="DockTreePanel"/> control tree.</summary>
    // We'd call these panels but DockPanel is already an Avalonia control and that would be confusing.
    public abstract partial class DockNodeViewModel : ObservableObject
    {
        /// <summary>Gets the id of the current instance.</summary>
        public String Id { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets a property dictionary usable for attacking implementation specific information
        /// to the current instance.
        /// </summary>
        public Dictionary<Object, Object> Tags { get; } = [];

        /// <summary>
        /// Searches from <see cref="DockNodeViewModel"/> to find a <see cref="DockTabNodeViewModel"/> which contains
        /// a <see cref="DockItemViewModel"/> with the given ID.
        /// </summary>
        /// <param name="itemId">The ID of the <see cref="DockItemViewModel"/> to find.</param>
        /// <returns>The <see cref="DockTabNodeViewModel"/> containing the <paramref name="itemId"/>, or <c>null</c> if not found.</returns>
        public DockTabNodeViewModel? FindAncestorTabNode(String itemId)
        {
            if (this is DockTabNodeViewModel tabNode)
            {
                DockItemViewModel? item = tabNode.Tabs.FirstOrDefault(item => item.Id == itemId);

                if (item is not null)
                {
                    return tabNode;
                }
            }
            else if (this is DockSplitNodeViewModel splitNode)
            {
                foreach (DockNodeViewModel child in splitNode.Children)
                {
                    DockTabNodeViewModel? childNode = child.FindAncestorTabNode(itemId);

                    if (childNode is not null)
                    {
                        return childNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Searches from <see cref="DockNodeViewModel"/> to find a <see cref="DockNodeViewModel"/> which contains
        /// a <see cref="DockSplitNodeViewModel"/> with the given ID.
        /// </summary>
        /// <param name="child">The <see cref="DockNodeViewModel"/> to find the parent for.</param>
        /// <returns>The nearest <see cref="DockSplitNodeViewModel"/> parent, or <c>null</c> if not found.</returns>
        public DockSplitNodeViewModel? FindAncestorSplitNode(DockNodeViewModel child)
        {
            if (this is DockSplitNodeViewModel splitNode)
            {
                foreach (DockNodeViewModel node in splitNode.Children)
                {
                    if (node == child)
                    {
                        return splitNode;
                    }

                    DockSplitNodeViewModel? result = node.FindAncestorSplitNode(child);
                    if (result != null)
                    {
                        return result;
                    }
                }

                return null;
            }
            else if (this is DockTabNodeViewModel)
            {
                // Tab nodes have no children to recurse into
                return null;
            }
            else
            {
                ////System.Diagnostics.Debug.Assert(false, "Unknown node type!");
                return null;
            }
        }

        /// <summary>
        /// Recursively searches a <see cref="DockNodeViewModel"/> tree for a <see cref="DockItemViewModel"/> with the given ID.
        /// </summary>
        /// <param name="itemId">The ID of the <see cref="DockItemViewModel"/> to find.</param>
        /// <returns>The matching <see cref="DockItemViewModel"/>, or <c>null</c> if not found.</returns>
        /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
        public T? FindDescendentItem<T>(String itemId)
            where T : DockItemViewModel
        {
            if (this is DockTabNodeViewModel tabNode)
            {
                return tabNode.Tabs.FirstOrDefault(item => item.Id == itemId) as T;
            }
            else if (this is DockSplitNodeViewModel splitNode)
            {
                foreach (DockNodeViewModel child in splitNode.Children)
                {
                    T? found = child.FindDescendentItem<T>(itemId);

                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches a <see cref="DockNodeViewModel"/> tree for a <see cref="DockNodeViewModel"/> with the given ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="DockNodeViewModel"/> to find.</param>
        /// <returns>The matching <see cref="DockNodeViewModel"/>, or <c>null</c> if not found.</returns>
        public DockNodeViewModel? FindDescendentNode(String id)
        {
            if (this.Id == id)
            {
                return this;
            }

            if (this is DockSplitNodeViewModel splitNode)
            {
                foreach (DockNodeViewModel child in splitNode.Children)
                {
                    DockNodeViewModel? found = child.FindDescendentNode(id);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively searches a <see cref="DockTabNodeViewModel"/> and returns the first <see cref="DockTabNodeViewModel"/> encountered.
        /// </summary>
        /// <returns>
        /// The first <see cref="DockTabNodeViewModel"/> found in the subtree, or <c>null</c> if no tab node exists.
        /// </returns>
        public DockTabNodeViewModel? FindFirstTabNode()
        {
            if (this is DockTabNodeViewModel tabNode)
            {
                return tabNode;
            }

            if (this is DockSplitNodeViewModel split)
            {
                foreach (DockNodeViewModel child in split.Children)
                {
                    DockTabNodeViewModel? result = child.FindFirstTabNode();
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}
