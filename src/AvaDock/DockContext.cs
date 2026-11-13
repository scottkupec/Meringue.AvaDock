// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia.Input;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock
{
    /// <summary>
    /// Context class for use with the docking controls.
    /// </summary>
    /// CONSIDER: This should possibly be public. Developers may need to inspect current item state, in particular, for decision making.
    internal class DockContext
    {
        /// <summary>
        /// The name of the <see cref="DataObject"/> context used for drag and drop of <see cref="DockItem"/>s.
        /// </summary>
        internal const String DragDropContextName = "AvaDock.SourceTabNode";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockWorkspaceManager"/> mapping.
        /// </summary>
        private const String DockWorkspaceManagerPropertyName = "AvaDock.WorkspaceManager";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemState"/> of a <see cref="DockItemViewModel"/>.
        /// </summary>
        private const String ItemState = "AvaDock.ItemState";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockTabNodeViewModel"/> mapping.
        /// </summary>
        private const String PreferredTabPanelIdPropertyName = "AvaDock.PreferredTabPanelId";

        /// <summary>
        /// The <see cref="DataFormat{T}"/> for dragging a <see cref="DockItemViewModel"/> in Avalonia 12.
        /// </summary>
        public static readonly DataFormat<DockItemViewModel> DockItemDragFormat =
            DataFormat.CreateInProcessFormat<DockItemViewModel>("application/x-meringuedockitem");

        /// <summary>
        /// Adds the specified state flags to the <see cref="ItemState"/> of the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose state is being modified.</param>
        /// <param name="state">The state flags to add.</param>
        public static void AddItemState(DockItemViewModel item, DockItemState state)
        {
            ArgumentNullException.ThrowIfNull(item);
            DockItemState current = GetItemState(item) ?? DockItemState.Normal;
            item.Tags[DockContext.ItemState] = current | state;
        }

        /// <summary>
        /// Clears all <see cref="ItemState"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose state is being cleared.</param>
        public static void ClearItemState(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _ = item.Tags.Remove(DockContext.ItemState);
        }

        /// <summary>
        /// Clears the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred panel ID is being cleared.</param>
        public static void ClearPreferredTabPanelId(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _ = item.Tags.Remove(DockContext.PreferredTabPanelIdPropertyName);
        }

        /// <summary>
        /// Clears the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose workspace is being cleared.</param>
        public static void ClearWorkspace(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            _ = item.Tags.Remove(DockContext.DockWorkspaceManagerPropertyName);
        }

        /// <summary>
        /// Clears the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockNodeViewModel"/>.
        /// </summary>
        /// <param name="node">The <see cref="DockNodeViewModel"/> whose workspace is being cleared.</param>
        public static void ClearWorkspace(DockNodeViewModel node)
        {
            ArgumentNullException.ThrowIfNull(node);
            _ = node.Tags.Remove(DockContext.DockWorkspaceManagerPropertyName);
        }

        /// <summary>
        /// Gets the <see cref="ItemState"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> for which to retrieve the <see cref="ItemState"/>.</param>
        /// <returns>
        /// The <see cref="ItemState"/>, or <c>null</c> if no state has been set.
        /// </returns>
        public static DockItemState? GetItemState(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return item.Tags.TryGetValue(DockContext.ItemState, out Object? value) && value is DockItemState state
                ? state
                : null;
        }

        /// <summary>
        /// Gets the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred panel ID is being retrieved.</param>
        /// <returns>
        /// The Id of the preferred <see cref="DockTabNodeViewModel"/> panel, or <c>null</c> if no preference has been set.
        /// </returns>
        public static String? GetPreferredTabPanelId(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return item.Tags.TryGetValue(DockContext.PreferredTabPanelIdPropertyName, out Object? value)
                ? value as String
                : null;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> for which to retrieve the <see cref="DockWorkspaceManager"/>.</param>
        /// <returns>
        /// The <see cref="DockWorkspaceManager"/>, or <c>null</c> if no host root has been set.
        /// </returns>
        public static DockWorkspaceManager? GetWorkspace(DockItemViewModel item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return item.Tags.TryGetValue(DockContext.DockWorkspaceManagerPropertyName, out Object? value)
                ? value as DockWorkspaceManager
                : null;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockNodeViewModel"/>.
        /// </summary>
        /// <param name="node">The <see cref="DockNodeViewModel"/> for which to retrieve the <see cref="DockWorkspaceManager"/>.</param>
        /// <returns>
        /// The <see cref="DockWorkspaceManager"/>, or <c>null</c> if no host root has been set.
        /// </returns>
        public static DockWorkspaceManager? GetWorkspace(DockNodeViewModel node)
        {
            ArgumentNullException.ThrowIfNull(node);
            return node.Tags.TryGetValue(DockContext.DockWorkspaceManagerPropertyName, out Object? value)
                ? value as DockWorkspaceManager
                : null;
        }

        /// <summary>
        /// Removes the specified state flags from the <see cref="ItemState"/> of the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose state is being modified.</param>
        /// <param name="state">The state flags to remove.</param>
        public static void RemoveItemState(DockItemViewModel item, DockItemState state)
        {
            ArgumentNullException.ThrowIfNull(item);
            DockItemState current = GetItemState(item) ?? DockItemState.Normal;

            if ((current & state) != state)
            {
                return;
            }

            current &= ~state;

            if (current == DockItemState.Normal)
            {
                _ = item.Tags.Remove(DockContext.ItemState);
            }
            else
            {
                item.Tags[DockContext.ItemState] = current;
            }
        }

        /// <summary>
        /// Sets the <see cref="ItemState"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose dock control is being set.</param>
        /// <param name="state">The <see cref="ItemState"/> to record for this item.</param>
        /// <remarks>
        /// This overwrites any previously recorded state. Use <see cref="AddItemState"/>
        /// or <see cref="RemoveItemState"/> to preserve the other state flags.
        /// </remarks>
        public static void SetItemState(DockItemViewModel item, DockItemState state)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.Tags[DockContext.ItemState] = state;
        }

        /// <summary>
        /// Sets the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// This value can be used later to restore or move the tab back to its intended panel.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred panel ID is being set.</param>
        /// <param name="tabPanelId">The Id of the preferred <see cref="DockTabNodeViewModel"/> panel to associate with this tab.</param>
        public static void SetPreferredTabPanelId(DockItemViewModel item, String tabPanelId)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.Tags[DockContext.PreferredTabPanelIdPropertyName] = tabPanelId;
        }

        /// <summary>
        /// Sets the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose dock control is being set.</param>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to associate with this tab.</param>
        public static void SetWorkspace(DockItemViewModel item, DockWorkspaceManager workspace)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.Tags[DockContext.DockWorkspaceManagerPropertyName] = workspace;
        }

        /// <summary>
        /// Sets the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="node">The <see cref="DockNodeViewModel"/> whose dock control is being set.</param>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to associate with this tab.</param>
        public static void SetWorkspace(DockNodeViewModel node, DockWorkspaceManager workspace)
        {
            ArgumentNullException.ThrowIfNull(node);
            node.Tags[DockContext.DockWorkspaceManagerPropertyName] = workspace;
        }
    }
}
