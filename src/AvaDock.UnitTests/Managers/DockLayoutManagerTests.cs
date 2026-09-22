// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.UnitTests;
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
            // Arrange
            DockLayoutManager manager = new();

            // Act
            Boolean applied = manager.ApplyLayout(null!);

            // Assert
            applied
                .ShouldBeFalse("ApplyLayout should return false when layout is null.");
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

            mergedHidden.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            mergedHidden.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");

            DockItemViewModel? restoredMinimized = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "min123");

            restoredMinimized
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restoredMinimized.Title
                .ShouldBe("Minimized Item", "Title should match serialized layout.");

            DockTabNodeViewModel? owningTab = manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("min123");

            owningTab
                .ShouldBeNull("Minimized item should not be part of any tab node.");
        }

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

            merged.Context
                .ShouldBe("HiddenContext", "Context from runtime hidden item should be preserved.");

            merged.Title
                .ShouldBe("Serialized Hidden", "Title should be updated from serialized layout.");
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

            DockItemViewModel? merged = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "minBoth");

            merged
                .ShouldNotBeNull("Minimized item should be present after merge.");

            merged.Context
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

            DockItemViewModel? inv = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "inv");
            inv
                .ShouldNotBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.FindOwningTabNode("inv").ShouldBeNull();

            manager.DockControl.PrimaryWorkspace.DockTree.Children
                .Count(c => c is DockTabNodeViewModel)
                .ShouldBe(1, "Only one tab node should exist after layout is applied.");
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

            restored.Context
                .ShouldBe("RuntimeContext", "Context should be preserved from runtime layout.");

            restored.Title
                .ShouldBe("Serialized Item", "Title should be updated from serialized layout.");
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
            DockContext.SetItemState(runtimeHidden, DockItemState.Hidden);
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

            merged.Context
                .ShouldBe("HiddenContext", "Context should be preserved from runtime layout.");

            merged.Title
                .ShouldBe("Runtime Hidden", "Title should match runtime layout.");
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

            DockItemViewModel? restored = manager.DockControl.PrimaryWorkspace.MinimizedItems.FirstOrDefault(i => i.Id == "min123");

            restored
                .ShouldNotBeNull("Minimized item should be restored into workspace.");

            restored.Title
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

            restored.Title
                .ShouldBe("Serialized Hidden", "Title should match serialized layout.");
        }

        [AvaloniaFact]
        public void CreateOrUpdateItem_CreatesNewItemWithDefaultParent()
        {
            // Arrange
            DockLayoutManager manager = new();

            // Act
            DockItemViewModel? created = manager.CreateOrUpdateItem("newId", "New Title", "new context", null);

            // Assert
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
        public void CreateOrUpdateItem_ThrowsWhenIdIsNullOrWhiteSpace()
        {
            // Arrange
            DockLayoutManager manager = new();

            // Act / Assert
            String message1 = Should.Throw<ArgumentException>(() => manager.CreateOrUpdateItem(null!, "title", new Object(), null))
                .Message;
            message1.Contains("Item ID cannot be null or whitespace", StringComparison.Ordinal)
                .ShouldBeTrue("Exception message should indicate invalid id.");

            String message2 = Should.Throw<ArgumentException>(() => manager.CreateOrUpdateItem("   ", "title", new Object(), null))
                .Message;
            message2.Contains("Item ID cannot be null or whitespace", StringComparison.Ordinal)
                .ShouldBeTrue("Exception message should indicate invalid id.");
        }

        [AvaloniaFact]
        public void CreateOrUpdateItem_ThrowsWhenParentNotFoundAndPolicyIsError()
        {
            // Arrange
            DockLayoutManager manager = new();
            manager.InsertPolicy = DockInsertPolicy.Error;

            // Act / Assert
            String message = Should.Throw<ArgumentOutOfRangeException>(() => manager.CreateOrUpdateItem("id", "title", new Object(), "non-existent-parent"))
                .Message;

            message.Contains("Parent could not be found", StringComparison.Ordinal)
                .ShouldBeTrue("Exception should indicate missing parent.");
        }

        [AvaloniaFact]
        public void CreateOrUpdateItem_UpdatesExistingItem()
        {
            // Arrange
            DockLayoutManager manager = new();
            DockItemViewModel existing = manager.CreateOrUpdateItem("id1", "Old Title", "old context", null)!;

            // Act
            DockItemViewModel? updated = manager.CreateOrUpdateItem("id1", "New Title", "new context", null);

            // Assert
            updated.ShouldNotBeNull("CreateOrUpdateItem should return an item.");
            updated!
                .ShouldBe(existing, "CreateOrUpdateItem should return the existing item instance.");

            updated!.Title
                .ShouldBe("New Title", "Title should be updated.");

            updated!.Context
                .ShouldBe("new context", "Context should be updated.");
        }

        [AvaloniaFact]
        public void CreateOrUpdateItem_UsesInsertPolicyCreateFirst()
        {
            // Arrange
            DockLayoutManager manager = new();
            manager.InsertPolicy = DockInsertPolicy.CreateFirst;
            manager.CreateOrUpdateItem("first", "First", "ctx", null);
            manager.CreateOrUpdateItem("second", "Second", "ctx", null);

            // Act
            manager.CreateOrUpdateItem("third", "Third", "ctx", null);

            // Assert
            DockSplitNodeViewModel? root = manager.DockControl.PrimaryWorkspace.DockTree;
            root.ShouldNotBeNull("Root should be a split node.");

            // Find the tab node that contains the "third" item
            DockNodeViewModel? thirdTabNode = null;
            foreach (DockNodeViewModel child in root.Children)
            {
                if (child is DockTabNodeViewModel tab && tab.Tabs.Any(t => t.Id == "third"))
                {
                    thirdTabNode = tab;
                    break;
                }
            }

            thirdTabNode.ShouldNotBeNull("A tab node containing the third item should exist.");
            thirdTabNode.ShouldBe(root.Children[0], "CreateFirst should insert at index 0.");
        }

        [AvaloniaFact]
        public void FloatItem_RoundTripPreservesItemViaSerialization()
        {
            // Arrange
            DockSplitNodeViewModel primaryTree = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(primaryTree);
            DockLayoutManager layoutManager = new();
            layoutManager.DockControl = new DockControlManager(primary);

            DockItemViewModel item = primaryTree.FindItem<DockItemViewModel>("item1")!;
            item.Title = "Original";

            // Act: save layout
            using System.IO.MemoryStream stream = new();
            layoutManager.SaveLayout(stream);
            stream.Position = 0;

            // Load into new manager
            DockLayoutManager loader = new();
            loader.LoadLayout(stream);

            // Assert
            DockItemViewModel? loaded = loader.DockControl.FindItem("item1");
            loaded.ShouldNotBeNull("Item should survive round-trip.");
            loaded.Title.ShouldBe("Original", "Item title should be preserved.");
        }

        [AvaloniaFact]
        public void SaveLayout_WritesToFileAndCanBeLoaded()
        {
            // Arrange
            DockLayoutManager manager = new();
            manager.CreateOrUpdateItem("item2", "Item 2", new Object(), null);
            String tempFile = System.IO.Path.GetTempFileName();

            try
            {
                // Act
                manager.SaveLayout(tempFile);
                DockLayoutManager loader = new();
                Boolean applied = loader.LoadLayout(tempFile);

                // Assert
                applied.ShouldBeTrue("Layout should be loaded from file.");
                loader.DockControl.FindItem("item2").ShouldNotBeNull("Item should persist after file save/load round-trip.");
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
            // Arrange
            DockLayoutManager manager = new();
            manager.CreateOrUpdateItem("item1", "Item 1", new Object(), null);

            // Act
            using System.IO.MemoryStream stream = new();
            manager.SaveLayout(stream);
            stream.Position = 0;
            Boolean applied = manager.ApplyLayout(manager.Serializer.Load(stream));

            // Assert
            applied.ShouldBeTrue("Layout should be applied from saved stream.");
            manager.DockControl.FindItem("item1").ShouldNotBeNull("Item should persist after save/load round-trip.");
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
