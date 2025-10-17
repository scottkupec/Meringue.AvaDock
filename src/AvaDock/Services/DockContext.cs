// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia.Input;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Context class for use with the docking controls.
    /// </summary>
    internal class DockContext
    {
        /// <summary>
        /// The name of the <see cref="DataObject"/> context used for drag and drop of <see cref="DockItem"/>s.
        /// </summary>
        internal const String DragDropContextName = "AvaDock.SourceTabNode";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockTabNodeViewModel"/> mapping.
        /// </summary>
        private const String DockControlManagerPropertyName = "AvaDock.ControlManager";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockWorkspaceManager"/> mapping.
        /// </summary>
        private const String DockWorkspaceManagerPropertyName = "AvaDock.WorkspaceManager";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockTabNodeViewModel"/> mapping.
        /// </summary>
        private const String PreferredTabPanelIdPropertyName = "AvaDock.PreferredTabPanelId";

        /// <summary>
        /// The name of the <see cref="DockItemViewModel.Tags"/> entry for tracking <see cref="DockItemViewModel"/> to <see cref="DockWorkspaceManager"/> mapping.
        /// </summary>
        private const String PreferredWorkspaceIdPropertyName = "AvaDock.PreferredWorkspaceId";

        /// <summary>
        /// Clears the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred panel ID is being cleared.</param>
        public static void ClearPreferredTabPanelId(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            _ = item.Tags.Remove(DockContext.PreferredTabPanelIdPropertyName);
        }

        /// <summary>
        /// Clears the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred workspace ID is being cleared.</param>
        public static void ClearPreferredWorkspaceId(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            _ = item.Tags.Remove(DockContext.PreferredWorkspaceIdPropertyName);
        }

        /// <summary>
        /// Clears the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose workspace is being cleared.</param>
        public static void ClearWorkspace(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            _ = item.Tags.Remove(DockContext.DockWorkspaceManagerPropertyName);
        }

        /// <summary>
        /// Gets the <see cref="DockControlManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> for which to retrieve the <see cref="DockControlManager"/>.</param>
        /// <returns>
        /// The <see cref="DockControlManager"/>, or <c>null</c> if no host root has been set.
        /// </returns>
        public static DockControlManager? GetDockHost(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            return item.Tags.TryGetValue(DockContext.DockControlManagerPropertyName, out Object? value)
                ? value as DockControlManager
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
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            return item.Tags.TryGetValue(DockContext.PreferredTabPanelIdPropertyName, out Object? value)
                ? value as String
                : null;
        }

        /// <summary>
        /// Gets the preferred <see cref="DockWorkspaceManager"/> Id for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred workspace ID is being retrieved.</param>
        /// <returns>
        /// The Id of the preferred <see cref="DockWorkspaceManager"/>, or <c>null</c> if no preference has been set.
        /// </returns>
        public static String? GetPreferredWorkspaceId(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);

            return item.Tags.TryGetValue(DockContext.PreferredWorkspaceIdPropertyName, out Object? value) ? value as String : null;
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
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            return item.Tags.TryGetValue(DockContext.DockWorkspaceManagerPropertyName, out Object? value)
                ? value as DockWorkspaceManager
                : null;
        }

        /// <summary>
        /// Sets the <see cref="DockControlManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose dock control is being set.</param>
        /// <param name="hostRoot">The <see cref="DockControlManager"/> to associate with this tab.</param>
        public static void SetDockHost(DockItemViewModel item, DockControlManager hostRoot)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            item.Tags[DockContext.DockControlManagerPropertyName] = hostRoot;
        }

        /// <summary>
        /// Sets the preferred <see cref="DockTabNodeViewModel"/> panel Id for the specified <see cref="DockItemViewModel"/>.
        /// This value can be used later to restore or move the tab back to its intended panel.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred panel ID is being set.</param>
        /// <param name="tabPanelId">The Id of the preferred <see cref="DockTabNodeViewModel"/> panel to associate with this tab.</param>
        public static void SetPreferredTabPanelId(DockItemViewModel item, String tabPanelId)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            item.Tags[DockContext.PreferredTabPanelIdPropertyName] = tabPanelId;
        }

        /// <summary>
        /// Sets the preferred <see cref="DockWorkspaceManager"/> Id for the specified <see cref="DockItemViewModel"/>.
        /// This value can be used later to restore or move the tab back to its intended workspace.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose preferred workspace ID is being set.</param>
        /// <param name="workspaceId">The Id of the preferred <see cref="DockWorkspaceManager"/> to associate with this tab.</param>
        public static void SetPreferredWorkspaceId(DockItemViewModel item, String workspaceId)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            item.Tags[DockContext.PreferredWorkspaceIdPropertyName] = workspaceId;
        }

        /// <summary>
        /// Sets the <see cref="DockWorkspaceManager"/> for the specified <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose dock control is being set.</param>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to associate with this tab.</param>
        public static void SetWorkspace(DockItemViewModel item, DockWorkspaceManager workspace)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            item.Tags[DockContext.DockWorkspaceManagerPropertyName] = workspace;
        }
    }
}
