// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Managers.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockLayoutManagerTests
    {
        [AvaloniaFact]
        public void ApplyLayout_ReturnsFalseWhenLayoutIsNull()
        {
            DockLayoutManager manager = new();
            Boolean applied = manager.ApplyLayout(null!);
            applied.ShouldBeFalse("ApplyLayout should return false when layout is null.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeHiddenAndMinimizedTabsCorrectly()
        {
            DockItemViewModel runtimeHidden = new()
            {
                Id = "hidden123",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

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
                Hidden = [hiddenItemData],
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = workspaceData,
            };

            DockLayoutManager manager = new();
            manager.DockControl.AddHiddenItem(runtimeHidden);

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing hidden and minimized items.");

            DockItemViewModel? mergedHidden = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hidden123");

            mergedHidden
                .ShouldNotBeNull("Hidden item should be present after merge.");

            mergedHidden.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            mergedHidden.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");

            DockItemViewModel? restoredMinimized = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "min123");

            restoredMinimized
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restoredMinimized.Title
                .ShouldBe("Minimized Item", "Title should match serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindAncestorTabNode("min123");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeHiddenItemsCorrectly()
        {
            DockItemViewModel hiddenItem = new()
            {
                Id = "hidden123",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

            DockLayoutManager manager = new();
            manager.DockControl.AddHiddenItem(hiddenItem);

            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                    Hidden =
                    [
                        new DockItemData
                        {
                            Id = "hidden123",
                            Title = "Serialized Hidden",
                        },
                    ],
                },
            };

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing hidden items.");

            DockItemViewModel? merged = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hidden123");

            merged
                .ShouldNotBeNull("Hidden item should be present after merge.");

            merged.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            merged.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldMergeMinimizedItemFromRuntimeAndSerializedLayout()
        {
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

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with minimized item in both runtime and serialized layout.");

            DockItemViewModel? merged = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "minBoth");

            merged
                .ShouldNotBeNull("Minimized item should be present after merge.");

            merged.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            merged.Title
                .ShouldBe("Serialized Minimized", "Title should be updated from serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindAncestorTabNode("minBoth");

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

            DockItemViewModel? inv = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "inv");
            inv
                .ShouldNotBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.FindAncestorTabNode("inv").ShouldBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.Children
                .Count(c => c is DockTabNodeViewModel)
                .ShouldBe(1, "Only one tab node should exist after layout is applied.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveContextWhenItemIdsMatch()
        {
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

            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = splitData,
                },
            };

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should return true for valid layout.");

            DockItemViewModel? restored = manager.DockControl.PrimaryWorkspace.Items.FirstOrDefault(i => i.Id == "abc123");

            restored
                .ShouldNotBeNull("Item with matching ID should be restored.");

            restored.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            restored.Title
                .ShouldBe("Serialized Item", "Title should be updated from serialized layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldPreserveRuntimeOnlyHiddenItem()
        {
            DockItemViewModel runtimeHidden = new()
            {
                Id = "hiddenOnly",
                Title = "Runtime Hidden",
                Context = "HiddenContext",
            };

            DockLayoutManager manager = new();
            DockContext.SetItemState(runtimeHidden, DockItemState.Hidden);
            manager.DockControl.AddHiddenItem(runtimeHidden);

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

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with runtime-only hidden item.");

            DockItemViewModel? merged = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hiddenOnly");

            merged
                .ShouldNotBeNull("Runtime-only hidden item should be preserved.");

            merged.Context
                .ShouldBe("HiddenContext", "Context should be preserved from runtime layout.");

            merged.Title
                .ShouldBe("Runtime Hidden", "Title should match runtime layout.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldRestoreMinimizedItemsCorrectly()
        {
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

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with valid layout containing minimized items.");

            DockItemViewModel? restored = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "min123");

            restored
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restored.Title
                .ShouldBe("Minimized Item", "Title should match serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindAncestorTabNode("min123");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

        [AvaloniaFact]
        public void ApplyLayout_ShouldRestoreSerializedOnlyHiddenItem()
        {
            DockItemData hiddenItemData = new()
            {
                Id = "hiddenSerialized",
                Title = "Serialized Hidden",
            };

            DockControlData layout = new()
            {
                PrimaryWorkspace = new DockWorkspaceData
                {
                    DockTree = new DockSplitNodeData
                    {
                        Orientation = Orientation.Horizontal,
                    },
                    Hidden = [hiddenItemData],
                },
            };

            DockLayoutManager manager = new();

            Boolean applied = manager.ApplyLayout(layout);

            applied
                .ShouldBeTrue("ApplyLayout should succeed with serialized-only hidden item.");

            DockItemViewModel? restored = manager.DockControl.HiddenItems.FirstOrDefault(i => i.Id == "hiddenSerialized");

            restored
                .ShouldNotBeNull("Serialized-only hidden item should be restored.");

            restored.Title
                .ShouldBe("Serialized Hidden", "Title should match serialized layout.");
        }

        [AvaloniaFact]
        public void CreateOrUpdateItem_CreatesNewItemWithDefaultParent()
        {
            DockLayoutManager manager = new();

            DockItemViewModel? created = manager.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "newId",
                    Title = "New Title",
                    Context = "new context",
                });

            created
                .ShouldNotBeNull("Item should be created.");

            created.Id
                .ShouldBe("newId", "Id should match.");

            created.Title
                .ShouldBe("New Title", "Title should match.");

            created.Context
                .ShouldBe("new context", "Context should match.");

            Boolean found = manager.DockControl.PrimaryWorkspace.Items.Any(i => i.Id == "newId");
            found
                .ShouldBeTrue("Item should be present in primary workspace.");
        }

        [AvaloniaFact]
        public void FloatItem_RoundTripPreservesItemViaSerialization()
        {
            DockSplitNodeViewModel primaryTree = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(primaryTree);
            DockLayoutManager layoutManager = new();
            layoutManager.DockControl = new DockControlManager(primary);

            DockItemViewModel item = primaryTree.FindDescendentItem<DockItemViewModel>("item1")!;
            item.Title = "Original";

            using System.IO.MemoryStream stream = new();
            layoutManager.SaveLayout(stream);
            stream.Position = 0;

            // Load into new manager
            DockLayoutManager loader = new();
            loader.LoadLayout(stream);

            DockItemViewModel? loaded = loader.DockControl.FindItem("item1");
            loaded.ShouldNotBeNull("Item should survive round-trip.");
            loaded.Title.ShouldBe("Original", "Item title should be preserved.");
        }

        [AvaloniaFact]
        public void SaveLayout_WritesToFileAndCanBeLoaded()
        {
            DockLayoutManager manager = new();

            DockItemViewModel? created = manager.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "item1",
                    Title = "Item 1",
                    Context = "new context",
                });

            String tempFile = System.IO.Path.GetTempFileName();

            try
            {
                manager.SaveLayout(tempFile);
                DockLayoutManager loader = new();
                Boolean applied = loader.LoadLayout(tempFile);

                applied.ShouldBeTrue("Layout should be loaded from file.");
                loader.DockControl.FindItem(created!.Id).ShouldNotBeNull("Item should persist after file save/load round-trip.");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }

        [AvaloniaFact]
        public void SaveLayout_WritesToStreamAndCanBeLoaded()
        {
            DockLayoutManager manager = new();
            DockItemViewModel? created = manager.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "item1",
                    Title = "Item 1",
                    Context = "new context",
                });

            using System.IO.MemoryStream stream = new();
            manager.SaveLayout(stream);
            stream.Position = 0;
            Boolean applied = manager.ApplyLayout(manager.Serializer.Load(stream));

            applied.ShouldBeTrue("Layout should be applied from saved stream.");
            manager.DockControl.FindItem(created!.Id).ShouldNotBeNull("Item should persist after save/load round-trip.");
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
