// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Provides services to convert between <see cref="DockControlData"/> and <see cref="DockControlManager"/> instances.
    /// </summary>
    internal static class DockSerializationConverter
    {
        /// <summary>
        /// Builds a <see cref="DockControlData"/> for the provided <see cref="DockControlManager"/>.
        /// </summary>
        /// <param name="control">The <see cref="DockControlManager"/> to be serialized.</param>
        /// <returns>The <paramref name="control"/> serialized as <see cref="DockControlData"/>.</returns>
        /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
        public static DockControlData BuildLayout<T>(DockControlManager control)
            where T : DockItemViewModel
        {
            return Serializer.Serialize<T>(control);
        }

        /// <summary>
        /// Builds a <see cref="DockControlManager"/> for the provided <see cref="DockControlData"/>.
        /// </summary>
        /// <param name="serializedControl">The <see cref="DockControlData"/> to be used.</param>
        /// <returns>The <see cref="DockControlManager"/> deserialized.</returns>
        /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
        public static DockControlManager BuildViewModel<T>(DockControlData serializedControl)
            where T : DockItemViewModel, new()
        {
            return Deserializer.Deserialize<T>(serializedControl);
        }

        /// <summary>
        /// Deserializes a <see cref="DockControlData"/> to a <see cref="DockControlManager"/>.
        /// </summary>
        private static class Deserializer
        {
            /// <summary>
            /// Builds a <see cref="DockControlManager"/> from a <see cref="DockControlData"/>.
            /// </summary>
            /// <param name="controlData">The <see cref="DockControlData"/> to be deserialized.</param>
            /// <returns>The <see cref="DockControlManager"/> built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
            public static DockControlManager Deserialize<T>(DockControlData controlData)
                where T : DockItemViewModel, new()
            {
                TargetFrameworkHelper.ThrowIfArgumentNull(controlData);
                TargetFrameworkHelper.ThrowIfArgumentNull(controlData.PrimaryWorkspace);

                if (controlData.Major != 1)
                {
                    throw new NotSupportedException($"Unsupported serialization version {controlData.Major}.{controlData.Minor}.{controlData.Patch}");
                }

                DockWorkspaceManager workspace = BuildWorkspace<T>(controlData.PrimaryWorkspace);
                DockControlManager control = new(workspace);

                if (controlData.Hidden is not null)
                {
                    foreach (DockItemData itemData in controlData.Hidden ?? [])
                    {
                        T item = BuildItem<T>(itemData);
                        control.AddHiddenItem(item);
                    }
                }

                if (controlData.SecondaryWorkspaces is not null)
                {
                    foreach (DockWindowData windowData in controlData.SecondaryWorkspaces)
                    {
                        if (windowData.Workspace is not null)
                        {
                            PixelPoint? position = null;

                            // Data may be deserialized onto a machine that has different displays than it was serialized from.
                            // Attempt to ensure any window is placed in a visible position.
                            PixelRect windowDisplayRect = new(windowData.Position, PixelSize.FromSize(windowData.Size, 1.0));
                            if (control.WindowManager!.MainWindow!.IsOnScreen(windowDisplayRect))
                            {
                                position = windowData.Position;
                            }

                            DockWorkspaceManager secondaryWorkspace = BuildWorkspace<T>(windowData.Workspace);
                            _ = control.AttachSecondaryWorkspace(secondaryWorkspace, position, windowData.Size);
                        }
                    }
                }

                return control;
            }

            /// <summary>
            /// Builds a <see cref="DockItemViewModel"/> based class from a <see cref="DockItemData"/> definition.
            /// </summary>
            /// <param name="itemData">The <see cref="DockItemData"/> to be converted to a <typeparamref name="T"/> instance.</param>
            /// <returns>The <typeparamref name="T"/> built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to build.</typeparam>
            private static T BuildItem<T>(DockItemData itemData)
                where T : DockItemViewModel, new()
            {
                String placeholderText = Application.Current?.Resources["AvaDockTabContentPlaceholderText"] as String ?? "Loading...";

                T item = new()
                {
                    DisableClose = itemData.DisableClose,
                    DisableHide = itemData.DisableHide,
                    DisableMaximize = itemData.DisableMaximize,
                    DisableMinimize = itemData.DisableMinimize,
                    // CONSIDER: How to have/restore context in cases where the application prefers to have it
                    //           already available.  Not our current use case, so punting for the moment.
                    Context = new TextBlock() { Text = placeholderText },
                    Id = itemData.Id,
                    Title = itemData.Title ?? placeholderText,
                };

                // CONSIDER: These two require special knowledge about the implementation
                //           details.  Can we make it more generic?  Do we just serialize
                //           all item.Tags?  What about tags that can't serialize?
                if (!String.IsNullOrWhiteSpace(itemData.Workspace))
                {
                    DockContext.SetPreferredWorkspaceId(item, itemData.Workspace!);
                }

                if (!String.IsNullOrWhiteSpace(itemData.Panel))
                {
                    DockContext.SetPreferredTabPanelId(item, itemData.Panel!);
                }

                return item;
            }

            /// <summary>
            /// Recurively builds a <see cref="DockNodeViewModel"/> from a <see cref="DockNodeData"/>.
            /// </summary>
            /// <param name="nodeData">The <see cref="DockNodeData"/> to be converted to a <see cref="DockNodeViewModel"/>.</param>
            /// <returns>The <see cref="DockNodeViewModel"/> that was built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "Not more readable here.")]
            private static DockNodeViewModel BuildNode<T>(DockNodeData nodeData)
                where T : DockItemViewModel, new()
            {
                TargetFrameworkHelper.ThrowIfArgumentNull(nodeData);
                DockNodeViewModel constructedBuildModel;

                if (nodeData is DockSplitNodeData splitNodeData)
                {
                    constructedBuildModel = BuildSplitNode<T>(splitNodeData);
                }
                else if (nodeData is DockTabNodeData tabNodeData)
                {
                    constructedBuildModel = BuildTabNode<T>(tabNodeData);
                }
                else
                {
                    throw new ArgumentException("Provided node could not be parsed.", nameof(nodeData));
                }

                return constructedBuildModel;
            }

            /// <summary>
            /// Builds a <see cref="DockSplitNodeViewModel"/> from a <see cref="DockSplitNodeData"/>.
            /// </summary>
            /// <param name="nodeData">The <see cref="DockSplitNodeData"/> to be converted to a <see cref="DockSplitNodeViewModel"/>.</param>
            /// <returns>The <see cref="DockSplitNodeViewModel"/> that was built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
            private static DockSplitNodeViewModel BuildSplitNode<T>(DockSplitNodeData nodeData)
                where T : DockItemViewModel, new()
            {
                TargetFrameworkHelper.ThrowIfArgumentNull(nodeData);

                DockSplitNodeViewModel split = new(nodeData.Orientation)
                {
                    Id = nodeData.Id ?? Guid.NewGuid().ToString("N"),
                };

                if (nodeData.Children?.Count > 0)
                {
                    List<Double> sizes = nodeData.Sizes ?? [.. Enumerable.Repeat(1.0, nodeData.Children.Count)];

                    if (sizes.Count != nodeData.Children.Count)
                    {
                        throw new InvalidOperationException($"Mismatch between child count ({nodeData.Children.Count}) and size count ({sizes.Count})");
                    }

                    for (Int32 i = 0; i < nodeData.Children.Count; i++)
                    {
                        DockNodeViewModel child = BuildNode<T>(nodeData.Children[i]);
                        split.InsertAt(i, child, sizes[i]);
                    }
                }

                return split;
            }

            /// <summary>
            /// Builds a <see cref="DockTabNodeViewModel"/> from a <see cref="DockTabNodeData"/>.
            /// </summary>
            /// <param name="nodeData">The <see cref="DockTabNodeData"/> to be converted to a <see cref="DockTabNodeViewModel"/>.</param>
            /// <returns>The <see cref="DockTabNodeViewModel"/> that was built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
            private static DockTabNodeViewModel BuildTabNode<T>(DockTabNodeData nodeData)
                where T : DockItemViewModel, new()
            {
                DockTabNodeViewModel tabNode = new()
                {
                    Id = nodeData.Id ?? Guid.NewGuid().ToString("N"),
                };

                foreach (DockItemData itemData in nodeData.Tabs ?? Enumerable.Empty<DockItemData>())
                {
                    T item = BuildItem<T>(itemData);
                    tabNode.AddTab(item);

                    if (nodeData.SelectedId != null && nodeData.SelectedId == itemData.Id)
                    {
                        tabNode.Selected = item;
                    }
                }

                return tabNode;
            }

            /// <summary>
            /// Builds a <see cref="DockWorkspaceManager"/> from a <see cref="DockWorkspaceData"/>.
            /// </summary>
            /// <param name="workspaceData">The <see cref="DockWorkspaceData"/> to be built.</param>
            /// <returns>The new <see cref="DockWorkspaceManager"/>.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to build.</typeparam>
            private static DockWorkspaceManager BuildWorkspace<T>(DockWorkspaceData workspaceData)
                where T : DockItemViewModel, new()
            {
                DockWorkspaceManager workspace;

                if (workspaceData.DockTree is not null)
                {
                    workspace = new(BuildSplitNode<T>(workspaceData.DockTree))
                    {
                        Id = workspaceData.Id ?? Guid.NewGuid().ToString("N"),
                    };

                    if (workspaceData.Minimized is not null)
                    {
                        foreach (DockItemData itemData in workspaceData.Minimized)
                        {
                            T item = BuildItem<T>(itemData);

                            if (workspace.AddItem(item))
                            {
                                if (item.MinimizeCommand.CanExecute(null))
                                {
                                    item.MinimizeCommand.Execute(null);
                                }
                            }
                        }
                    }
                }
                else
                {
                    workspace = new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Horizontal))
                    {
                        Id = workspaceData.Id ?? Guid.NewGuid().ToString("N"),
                    };
                }

                return workspace;
            }
        }

        /// <summary>
        /// Serializes a <see cref="DockControlManager"/> to a <see cref="DockControlData"/>.
        /// </summary>
        private static class Serializer
        {
            /// <summary>
            /// Builds a <see cref="DockControlData"/> from a <see cref="DockControlManager"/>.
            /// </summary>
            /// <param name="control">The <see cref="DockControlManager"/> to be serialized.</param>
            /// <returns>The <see cref="DockControlData"/> built.</returns>
            /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
            public static DockControlData Serialize<T>(DockControlManager control)
                where T : DockItemViewModel
            {
                TargetFrameworkHelper.ThrowIfArgumentNull(control);

                DockControlData controlData = new()
                {
                    PrimaryWorkspace = BuildWorkspaceData(control.PrimaryWorkspace),
                };

                if (control.HiddenItems.Any())
                {
                    controlData.Hidden = [.. BuildItemDataList(control.HiddenItems)];
                }

                if (control.SecondaryWorkspaces.Any())
                {
                    // CONSIDER: This requires knowledge of dock control utilizing windows.  What if
                    //           we ever need to support some other way of displaying secondary workspaces?
                    controlData.SecondaryWorkspaces = [.. BuildWindowDataList(control.WindowManager.Windows)];
                }

                return controlData;
            }

            /// <summary>
            /// Converts a <see cref="DockItemViewModel"/> into a <see cref="DockItemData"/> for serialization.
            /// </summary>
            /// <param name="item">The <see cref="DockItemViewModel"/> to be serialized to a <see cref="DockItemData"/>.</param>
            /// <returns>The <see cref="DockItemData"/> built.</returns>
            private static DockItemData BuildItemData(DockItemViewModel item)
            {
                DockItemData layout = new()
                {
                    Id = item.Id.ToString(),
                    DisableClose = item.DisableClose,
                    DisableHide = item.DisableHide,
                    DisableMaximize = item.DisableMaximize,
                    DisableMinimize = item.DisableMinimize,
                    Title = item.Title,
                };

                // CONSIDER: These two require special knowledge about the implementation
                //           details.  Can we make it more generic?  Do we just serialize
                //           all item.Tags?  What about tags that can't serialize?
                String? restoreToWorkspace = DockContext.GetPreferredWorkspaceId(item);

                if (restoreToWorkspace is not null)
                {
                    layout.Workspace = restoreToWorkspace;
                }

                String? restoreToPanel = DockContext.GetPreferredTabPanelId(item);

                if (restoreToPanel is not null)
                {
                    layout.Panel = restoreToPanel;
                }

                return layout;
            }

            /// <summary>
            /// Helper method for calling <see cref="BuildItemData"/> on a enumeration of <see cref="DockItemViewModel"/>s.
            /// </summary>
            /// <param name="itemList">The enumeration of <see cref="DockItemViewModel"/>s to serialize.</param>
            /// <returns>An enumeration of <see cref="DockItemData"/>s representing the <paramref name="itemList"/>.</returns>
            private static IEnumerable<DockItemData> BuildItemDataList(IEnumerable<DockItemViewModel> itemList)
            {
                foreach (DockItemViewModel item in itemList)
                {
                    yield return BuildItemData(item);
                }
            }

            /// <summary>
            /// Recursively converts a tree of <see cref="DockNodeViewModel"/> into a tree of <see cref="DockNodeData"/>
            /// for serialization.
            /// </summary>
            /// <param name="node">The <see cref="DockNodeViewModel"/> to be serialized to a <see cref="BuildNodeData"/>.</param>
            /// <returns>The <see cref="DockNodeData"/> built.</returns>
            private static DockNodeData BuildNodeData(DockNodeViewModel node)
            {
                return node switch
                {
                    DockSplitNodeViewModel splitNode =>
                        new DockSplitNodeData
                        {
                            Id = splitNode.Id,
                            Orientation = splitNode.Orientation,
                            Children = [.. splitNode.Children.Select(BuildNodeData)],
                            Sizes = NormalizeSizes(splitNode.Sizes),
                        },

                    DockTabNodeViewModel tabNode =>
                        new DockTabNodeData
                        {
                            Id = tabNode.Id,
                            SelectedId = tabNode.Selected?.Id,
                            Tabs = [.. tabNode.Tabs.Select(BuildItemData)],
                        },

                    _ => throw new NotSupportedException($"Unknown node type: {node.GetType().Name}"),
                };
            }

            /// <summary>
            /// Converts a <see cref="DockItemViewModel"/> into a <see cref="DockItemData"/> for serialization.
            /// </summary>
            /// <param name="window">The <see cref="Window"/> to be serialized to a <see cref="DockWindowData"/>.</param>
            /// <returns>The <see cref="DockWindowData"/> built.</returns>
            private static DockWindowData BuildWindowData(IWindow window)
            {
                System.Diagnostics.Debug.Assert(window.DataContext is DockWorkspaceManager, "Invalid DataContext on provided window.");

                return new DockWindowData()
                {
                    Workspace = BuildWorkspaceData((window.DataContext as DockWorkspaceManager)!),
                    Height = window.Height,
                    Left = window.Position.X,
                    Top = window.Position.Y,
                    Width = window.Width,
                };
            }

            /// <summary>
            /// Helper method for calling <see cref="BuildWindowData"/> on a enumeration of <see cref="Window"/>s.
            /// </summary>
            /// <param name="windowList">The enumeration of <see cref="Window"/>s to serialize.</param>
            /// <returns>An enumeration of <see cref="DockWindowData"/>s representing the <paramref name="windowList"/>.</returns>
            /// <remarks>Each <see cref="Window"/> is assumed to have a DataContext derived from <see cref="DockWorkspaceManager"/>.</remarks>
            private static IEnumerable<DockWindowData> BuildWindowDataList(IEnumerable<IWindow> windowList)
            {
                foreach (IWindow window in windowList)
                {
                    yield return BuildWindowData(window);
                }
            }

            /// <summary>
            /// Converts a <see cref="DockWorkspaceManager"/> into a <see cref="DockWorkspaceData"/> for serialization.
            /// </summary>
            /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to be serialized to a <see cref="DockWorkspaceData"/>.</param>
            /// <returns>The <see cref="DockWorkspaceData"/> built.</returns>
            private static DockWorkspaceData BuildWorkspaceData(DockWorkspaceManager workspace)
            {
                DockWorkspaceData workspaceData = new()
                {
                    Id = workspace.Id,
                    DockTree = BuildNodeData(workspace.DockTree) as DockSplitNodeData,
                };

                if (workspace.MinimizedItems.Any())
                {
                    workspaceData.Minimized = [.. BuildItemDataList(workspace.MinimizedItems)];
                }

                return workspaceData;
            }

            /// <summary>Adjusts <paramref name="sizes"/> values to sum to 1.</summary>
            /// <param name="sizes">The sizes to normalize.</param>
            /// <returns>The normalized <paramref name="sizes"/>.</returns>
            private static List<Double> NormalizeSizes(ReadOnlyObservableCollection<Double> sizes)
            {
                Double total = sizes.Sum();
                List<Double> normalizedSizes = [];

                if (total > 0)
                {
                    for (Int32 i = 0; i < sizes.Count; i++)
                    {
                        normalizedSizes.Add(sizes[i] / total);
                    }
                }

                return normalizedSizes;
            }
        }
    }
}
