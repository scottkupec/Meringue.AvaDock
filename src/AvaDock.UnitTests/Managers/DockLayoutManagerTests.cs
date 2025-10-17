// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.UnitTests;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Managers.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockLayoutManagerTests
    {
        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeHiddenItemsCorrectly()
        {
            // Arrange: runtime layout with one hidden item
            DockItemViewModel hiddenItem = new()
            {
                Id = "hidden123",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

            DockLayoutManager manager = new();
            manager.DockControl.AddHiddenItem(hiddenItem);

            // Arrange: serialized layout with same hidden item (no context)
            DockControlData layout = new()
            {
                Hidden =
                [
                    new DockItemData
                    {
                        Id = "hidden123",
                        Title = "Serialized Hidden",
                    },
                ],
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                },
            };

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing hidden items.");

            DockItemViewModel? merged = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hidden123");

            merged
                .ShouldNotBeNull("Hidden item should be present after merge.");

            merged!.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            merged.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeHiddenAndMinimizedTabsCorrectly()
        {
            // Arrange: runtime layout with one hidden item
            DockItemViewModel runtimeHidden = new()
            {
                Id = "hidden123",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

            // Arrange: serialized layout with one minimized item and one hidden item (same ID as runtime)
            DockItemData minimizedItemData = new()
            {
                Id = "min123",
                Title = "Minimized Item",
            };

            DockItemData hiddenItemData = new()
            {
                Id = "hidden123",
                Title = "Serialized Hidden",
            };

            DockWorkspaceData workspaceData = new()
            {
                DockTree = new DockSplitNodeData
                {
                    Orientation = Orientation.Horizontal,
                },
                Minimized = [minimizedItemData],
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = workspaceData,
                Hidden = [hiddenItemData],
            };

            DockLayoutManager manager = new();
            manager.DockControl.AddHiddenItem(runtimeHidden);

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing hidden and minimized items.");

            DockItemViewModel? mergedHidden = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hidden123");

            mergedHidden
                .ShouldNotBeNull("Hidden item should be present after merge.");

            mergedHidden!.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            mergedHidden.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");

            DockItemViewModel? restoredMinimized = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "min123");

            restoredMinimized
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restoredMinimized!.Title
                .ShouldBe("Minimized Item", "Title should match serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("min123");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeMinimizedItemFromRuntimeAndSerializedLayout()
        {
            // Arrange: runtime layout with one minimized item
            DockItemViewModel runtimeMinimized = new()
            {
                Id = "minBoth",
                Title = "Runtime Minimized",
                Context = "RuntimeContext",
            };

            DockWorkspaceManager runtimeWorkspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            _ = runtimeWorkspace.AddItem(runtimeMinimized);
            runtimeMinimized.MinimizeCommand.Execute(null);

            DockLayoutManager manager = new();
            manager.DockControl = new DockControlManager(runtimeWorkspace);

            // Arrange: serialized layout with same item ID
            DockItemData minimizedItemData = new()
            {
                Id = "minBoth",
                Title = "Serialized Minimized",
            };

            DockWorkspaceData workspaceData = new()
            {
                DockTree = new DockSplitNodeData
                {
                    Orientation = Orientation.Horizontal,
                },
                Minimized = [minimizedItemData],
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = workspaceData,
            };

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with minimized item in both runtime and serialized layout.");

            DockItemViewModel? merged = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "minBoth");

            merged
                .ShouldNotBeNull("Minimized item should be present after merge.");

            merged!.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            merged.Title
                .ShouldBe("Serialized Minimized", "Title should be updated from serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("minBoth");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldNotCreateExtraTabNodesForMinimizedItems()
        {
            DockItemData minimizedItemData = new()
            {
                Id = "inv",
                Title = "Inventory",
                Panel = "left",
            };

            DockSplitNodeData nestedSplit = new()
            {
                Orientation = Orientation.Horizontal,
                Children = [
                    new DockTabNodeData
                    {
                        Id = "right",
                        Tabs =
                        [
                            new() { Id = "bounty", Title = "Bounties" },
                        ],
                    }
                ],
                Sizes = [1],
            };

            DockSplitNodeData rootSplit = new()
            {
                Orientation = Orientation.Horizontal,
                Children = [nestedSplit],
                Sizes = [1],
            };

            DockWorkspaceData workspaceData = new()
            {
                DockTree = rootSplit,
                Minimized = [minimizedItemData],
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = workspaceData,
            };

            DockLayoutManager manager = new();
            Boolean applied = manager.ApplyLayout(layout);

            applied.ShouldBeTrue();

            DockItemViewModel? inv = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "inv");
            inv
                .ShouldNotBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("inv").ShouldBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.Children
                .Count(c => c is DockTabNodeViewModel)
                .ShouldBe(1, "Only one tab node should exist after layout is applied.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveHiddenAndMinimizedTabs()
        {
            DockControlData layout = DockLayout.Layout(
                primary: DockLayout.Workspace(
                    dockTree: DockLayout.Horizontal(
                        DockLayout.Vertical(
                            DockLayout.Tab(
                                "left-1",
                                DockLayout.Item("left-1-item-1", "left-1-title-1"),
                                DockLayout.Item("left-1-item-2", "left-1-title-2")),
                            DockLayout.Tab("left-2", DockLayout.Item("left-2-item-1", "left-2-item-1"))),
                        DockLayout.Vertical(
                            DockLayout.Tab("center-1", DockLayout.Item("center-1-item-1", "center-1-title-1")),
                            DockLayout.Tab("center-2", DockLayout.Item("center-2-item-1", "center-2-title-1"))),
                        DockLayout.Vertical(
                            DockLayout.Tab("right-1", DockLayout.Item("right-1-item-1", "right-1-title-1")),
                            DockLayout.Horizontal(
                                DockLayout.Tab("right-2", DockLayout.Item("right-2-item-1", "right-2-title-1")),
                                DockLayout.Tab("right-3", DockLayout.Item("right-3-item-1", "right-3-title-1")),
                                DockLayout.Vertical(
                                    DockLayout.Tab("right-4", DockLayout.Item("right-4-item-1", "right-4-title-1")),
                                    DockLayout.Tab("right-5", DockLayout.Item("right-5-item-1", "right-5-title-1")))))),
                    minimizedItems: DockLayout.Minimized(
                        DockLayout.Item("minimized-1", "minimized-title-1", "left-2"),
                        DockLayout.Item("minimized-2", "minimized-title-2", "left-3"))),
                hiddenItems: DockLayout.Hidden(
                    DockLayout.Item("hidden-1", "hidden-title-1", "right-4"),
                    DockLayout.Item("hidden-2", "hidden-title-2", "center-2")));

            DockLayoutManager manager = new();
            Boolean applied = manager.ApplyLayout(layout);
            applied
                .ShouldBeTrue("Layout should be successfully applied.");

            DockWorkspaceManager workspace = manager.DockControl.PrimaryWorkspace;

            // Minimized item validation
            manager.DockControl.FindOwningTabNode("minimized-1")
                .ShouldBeNull("Minimized item 'minimized-1' should not be docked.");

            manager.DockControl.FindItem("minimized-1")
                .ShouldNotBeNull("Minimized item 'minimized-1' should be be discoverable in Items.");

            // Hidden item validation
            manager.DockControl.FindItem("hidden-1")
                .ShouldNotBeNull("Hidden item 'hidden-1' should be be discoverable in Items.");

            manager.DockControl.FindOwningTabNode("hidden-1")
                .ShouldBeNull("Hidden item 'hidden-1' should not be docked.");

            // Validate the total number of tab nodes.
            Int32 totalTabNodes = CountTabNodes(workspace.DockTree);
            totalTabNodes
                .ShouldBe(9, "Layout should contain exactly 9 tab nodes.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveRuntimeOnlyHiddenItem()
        {
            // Arrange: runtime layout with one hidden item
            DockItemViewModel runtimeHidden = new()
            {
                Id = "hiddenOnly",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

            DockLayoutManager manager = new();
            manager.DockControl.AddHiddenItem(runtimeHidden);

            // Arrange: serialized layout with no hidden items
            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                },
            };

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with runtime-only hidden item.");

            DockItemViewModel? merged = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hiddenOnly");

            merged
                .ShouldNotBeNull("Runtime-only hidden item should be preserved.");

            merged!.Context
                .ShouldBe("HiddenContext", "Context should be preserved from runtime layout.");

            merged.Title
                .ShouldBe("Runtime Hidden", "Title should match runtime layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveRuntimeOnlyMinimizedItem()
        {
            // Arrange: runtime layout with one minimized item
            DockItemViewModel runtimeMinimized = new()
            {
                Id = "minOnly",
                Title = "Runtime Minimized",
                Context = "RuntimeContext",
            };

            DockWorkspaceManager runtimeWorkspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            _ = runtimeWorkspace.AddItem(runtimeMinimized);
            runtimeMinimized.MinimizeCommand.Execute(null);

            DockLayoutManager manager = new();
            manager.DockControl = new DockControlManager(runtimeWorkspace);

            // Arrange: serialized layout with no minimized items
            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                },
            };

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with runtime-only minimized item.");

            DockItemViewModel? restored = manager.DockControl.FindItem(runtimeMinimized.Id);

            restored
                .ShouldNotBeNull("Runtime-only minimized item should be preserved.");

            restored!.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            restored.Title
                .ShouldBe("Runtime Minimized", "Title should match runtime layout.");

            DockWorkspaceManager? workspace = manager.DockControl.GetWorkspace(runtimeMinimized);

            workspace
                .ShouldBeNull("Items not part of the applied layout should be hidden.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldRestoreMinimizedItemsCorrectly()
        {
            // Arrange: serialized layout with one minimized item
            DockItemData minimizedItemData = new()
            {
                Id = "min123",
                Title = "Minimized Item",
            };

            DockWorkspaceData workspaceData = new()
            {
                DockTree = new DockSplitNodeData
                {
                    Orientation = Orientation.Horizontal,
                },
                Minimized = [minimizedItemData],
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = workspaceData,
            };

            DockLayoutManager manager = new();

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing minimized items.");

            DockItemViewModel? restored = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "min123");

            restored
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restored!.Title
                .ShouldBe("Minimized Item", "Title should match serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("min123");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldRestoreSerializedOnlyHiddenItem()
        {
            // Arrange: serialized layout with one hidden item
            DockItemData hiddenItemData = new()
            {
                Id = "hiddenSerialized",
                Title = "Serialized Hidden",
            };

            DockControlData layout = new()
            {
                Hidden = [hiddenItemData],
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                },
            };

            DockLayoutManager manager = new();

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should succeed with serialized-only hidden item.");

            DockItemViewModel? restored = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hiddenSerialized");

            restored
                .ShouldNotBeNull("Serialized-only hidden item should be restored.");

            restored!.Title
                .ShouldBe("Serialized Hidden", "Title should match serialized layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveContextWhenItemIdsMatch()
        {
            // Arrange: runtime layout with one item
            DockItemViewModel runtimeItem = new()
            {
                Id = "abc123",
                Title = "Runtime Item",
                Context = "RuntimeContext",
            };

            DockTabNodeViewModel runtimeTabNode = new();
            runtimeTabNode.AddTab(runtimeItem);

            DockSplitNodeViewModel runtimeSplit = new(Orientation.Horizontal);
            runtimeSplit.AddChild(runtimeTabNode);

            DockWorkspaceManager runtimeWorkspace = new(runtimeSplit);
            DockLayoutManager manager = new();
            manager.DockControl = new DockControlManager(runtimeWorkspace);

            DockSplitNodeData splitData = new();
            splitData.Sizes.Add(1);
            splitData.Children.Add(
                new DockTabNodeData
                {
                    Id = "panel1",
                    Tabs =
                    {
                        new DockItemData
                        {
                            Id = "abc123",
                            Title = "Serialized Item",
                        },
                    },
                });

            // Arrange: serialized layout with same item ID but no context
            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = splitData,
                },
            };

            // Act
            Boolean applied = manager.ApplyLayout(layout);

            // Assert
            applied
                .ShouldBeTrue("ApplyLayout should return true for valid layout.");

            DockItemViewModel? restored = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "abc123");

            restored
                .ShouldNotBeNull("Item with matching ID should be restored.");

            restored!.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            restored.Title
                .ShouldBe("Serialized Item", "Title should be updated from serialized layout.");
        }

        private static Int32 CountTabNodes(DockNodeViewModel node)
        {
            return node is DockTabNodeViewModel
                ? 1
                : node is DockSplitNodeViewModel split
                    ? split.Children.Sum(CountTabNodes)
                    : 0;
        }
    }
}
