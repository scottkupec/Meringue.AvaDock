// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.UnitTests;
using Meringue.AvaDock.ViewModels;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Managers.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockControlManagerTests
    {
        [Fact]
        public void AddHiddenItem_AppendsToHiddenItems()
        {
            DockItemViewModel item = new()
            {
                Id = "hidden1",
                Title = "Hidden Item",
                Context = new Object(),
            };

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            DockControlManager manager = new(workspace);

            manager.AddHiddenItem(item);

            manager.HiddenItems
                .ShouldContain(item, "HiddenItems should contain the item after AddHiddenItem is called.");
        }

        [Fact]
        public void CloseItem_RemovesItemFromTabNode()
        {
            DockItemViewModel item = new()
            {
                Id = "item1",
                Title = "Item 1",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            manager.CloseItem(item);

            tabNode.Tabs
                .ShouldNotContain(item, "Item should be removed from tab node after CloseItem is called.");
        }

        [Fact]
        public void FindItem_LocatesItemInPrimaryWorkspace()
        {
            DockItemViewModel item = new()
            {
                Id = "findme",
                Title = "Find Me",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockItemViewModel? found = manager.FindItem("findme");

            found
                .ShouldBe(item, "FindItem should return the item from the primary workspace.");
        }

        [AvaloniaFact]
        public void MoveItem_DropDifferentCenterWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item1"),
                DockTree.Tab("item2"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Tab("item1", "item2"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("item2")!,
                    target: initialTree.FindOwningTabNode("item1")!,
                    zone: DropZone.Center)
                .ExpectTree(expectedTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_DropNoneWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item1", "item2"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("item1")!,
                    target: initialTree.FindOwningTabNode("item1")!,
                    zone: DropZone.None)
                .ExpectTree(initialTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_DropSameCenterWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item1", "item2"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("item1")!,
                    target: initialTree.FindOwningTabNode("item1")!,
                    zone: DropZone.Center)
                .ExpectTree(initialTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_HandlesGrandparentSplitChanges()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("move-item"),
                    DockTree.Tab("left-item")),
                DockTree.Vertical(
                    DockTree.Tab("center-item"),
                    DockTree.Tab("right-item")));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Horizontal(
                        DockTree.Tab("move-item"),
                        DockTree.Tab("right-item")),
                    DockTree.Tab("left-item")),
                DockTree.Tab("center-item"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("right-item")!,
                    target: initialTree.FindOwningTabNode("move-item")!,
                    zone: DropZone.Right)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [AvaloniaFact]
        public void MoveItem_MovingLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple items
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            DockControlManager manager = new(new DockWorkspaceManager(initialTree));

            // Create a secondary workspace with an empty root
            DockSplitNodeViewModel secondaryRoot = DockTree.Horizontal();
            DockWorkspaceManager secondaryWorkspace = new(secondaryRoot);
            manager.AttachSecondaryWorkspace(secondaryWorkspace, null, new Size(300, 200));

            // Act: move each item from the primary workspace to the secondary
            foreach (DockTabNodeViewModel tabNode in initialTree.Children.OfType<DockTabNodeViewModel>().ToList())
            {
                foreach (DockItemViewModel item in tabNode.Tabs.ToList())
                {
                    DockTabNodeViewModel targetTab = new();
                    Boolean result = manager.MoveItem(item, targetTab, new DockItemMoveOptions { DropZone = DropZone.Center });
                    result
                        .ShouldBeTrue($"Move of '{item.Id}' should succeed.");
                }
            }

            // Assert: PrimaryWorkspace still has a valid DockTabNodeViewModel
            DockNodeViewModel root = manager.PrimaryWorkspace.DockTree;
            root
                .ShouldBeOfType<DockSplitNodeViewModel>("PrimaryWorkspace root should be a split node.");

            DockSplitNodeViewModel split = (DockSplitNodeViewModel)root;
            split.Children.Count
                .ShouldBe(1, "PrimaryWorkspace should contain one fallback tab node.");

            split.Children[0]
                .ShouldBeOfType<DockTabNodeViewModel>("Fallback node should be a DockTabNodeViewModel.");
        }

        [AvaloniaFact]
        public void MoveItem_SplitBottomWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("top-item", "bottom-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("top-item"),
                    DockTree.Tab("bottom-item")));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("bottom-item")!,
                    target: initialTree.FindOwningTabNode("bottom-item")!,
                    zone: DropZone.Bottom)
                .ExpectTree(expectedTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_SplitDropRecomputesTargetIndexAfterRemove()
        {
            // "move-item" will be removed during move causing the index
            // of the tab containing "target-item" to become invalid.
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("move-item"),
                DockTree.Tab("target-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Tab("target-item"),
                DockTree.Tab("move-item"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("move-item")!,
                    target: initialTree.FindOwningTabNode("target-item")!,
                    zone: DropZone.Right)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [AvaloniaFact]
        public void MoveItem_SplitLeftWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("left-item", "right-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Tab("left-item"),
                DockTree.Tab("right-item"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("left-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    zone: DropZone.Left)
                .ExpectTree(expectedTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_SplitRightWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("left-item", "right-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Tab("left-item"),
                DockTree.Tab("right-item"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("right-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    zone: DropZone.Right)
                .ExpectTree(expectedTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_SplitTopWorks()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("top-item", "bottom-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("top-item"),
                    DockTree.Tab("bottom-item")));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("top-item")!,
                    target: initialTree.FindOwningTabNode("bottom-item")!,
                    zone: DropZone.Top)
                .ExpectTree(expectedTree)
                .Assert(displayActualTree: true);
        }

        [AvaloniaFact]
        public void MoveItem_TopOfCenterColumnCreatesVerticalSplit()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("move-item"),
                    DockTree.Tab("left-item")),
                DockTree.Tab("center-item"),
                DockTree.Tab("right-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("move-item"),
                    DockTree.Tab("left-item")),
                DockTree.Vertical(
                    DockTree.Tab("right-item"),
                    DockTree.Tab("center-item")));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("right-item")!,
                    target: initialTree.FindOwningTabNode("center-item")!,
                    zone: DropZone.Top)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [AvaloniaFact]
        public void MoveItem_TopOfLeftColumnCreatesVerticalSplit()
        {
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("left-item"),
                DockTree.Tab("center-item"),
                DockTree.Tab("right-item", "move-item"));

            DockSplitNodeViewModel expectedTree = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("move-item"),
                    DockTree.Tab("left-item")),
                DockTree.Tab("center-item"),
                DockTree.Tab("right-item"));

            new DockMutationTestBuilder()
                .WithInitialTree(initialTree)
                .WithMoveItem(
                    item: initialTree.FindItem<DockItemViewModel>("move-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    zone: DropZone.Top)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [Fact]
        public void OnTabCloseRequested_RemovesItemFromHiddenItems()
        {
            DockItemViewModel item = new()
            {
                Id = "close1",
                Title = "Closable",
                Context = new Object(),
            };

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            DockControlManager manager = new(workspace);
            manager.AddHiddenItem(item);

            manager.HiddenItems
                .ShouldContain(item, "Item should be in HiddenItems before CloseCommand is executed.");

            item.CloseCommand.Execute(null);

            manager.HiddenItems
                .ShouldNotContain(item, "Item should be removed from HiddenItems after CloseCommand is executed.");
        }

        [Fact]
        public void OnTabCloseRequested_RemovesItemFromSecondaryWorkspace()
        {
            DockItemViewModel item = new()
            {
                Id = "close2",
                Title = "Closable Secondary",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager secondaryWorkspace = new(split);
            DockControlManager manager = new(new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Horizontal)));
            manager.WindowManager = new WindowManager(new TestWindowFactory());

            manager.AttachSecondaryWorkspace(secondaryWorkspace, null, new Size(300, 200));

            secondaryWorkspace.Items
                .ShouldContain(item, "Item should be present in SecondaryWorkspace before CloseCommand is executed.");

            item.CloseCommand.Execute(null);

            secondaryWorkspace.Items
                .ShouldNotContain(item, "Item should be removed from SecondaryWorkspace after CloseCommand is executed.");
        }

        [AvaloniaFact]
        public void OnTabHideRequested_ForLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple tab nodes
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            DockControlManager manager = new(new DockWorkspaceManager(initialTree));

            // Act: hide all items in the primary workspace
            foreach (DockTabNodeViewModel tabNode in initialTree.Children.OfType<DockTabNodeViewModel>().ToList())
            {
                foreach (DockItemViewModel item in tabNode.Tabs.ToList())
                {
                    item.HideCommand.Execute(null);
                }
            }

            // Assert: PrimaryWorkspace still has a valid DockTabNodeViewModel
            DockNodeViewModel root = manager.PrimaryWorkspace.DockTree;
            root
                .ShouldBeOfType<DockSplitNodeViewModel>("PrimaryWorkspace root should be a split node.");

            DockSplitNodeViewModel split = (DockSplitNodeViewModel)root;
            split.Children.Count
                .ShouldBe(1, "PrimaryWorkspace should contain one fallback tab node.");

            split.Children[0]
                .ShouldBeOfType<DockTabNodeViewModel>("Fallback node should be a DockTabNodeViewModel.");
        }

        [Fact]
        public void OnTabHideRequested_MovesItemFromPrimaryWorkspaceToHiddenItems()
        {
            DockItemViewModel item = new()
            {
                Id = "hide1",
                Title = "Hideable",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            manager.HiddenItems
                .ShouldNotContain(item, "Sanity: Item should not be in HiddenItems before HideCommand is executed.");

            workspace.Items
                .ShouldContain(item, "Sanity: Item should be present in workspace before HideCommand is executed.");

            item.HideCommand.Execute(null);

            manager.HiddenItems
                .ShouldContain(item, "Item should be added to HiddenItems after HideCommand is executed.");

            workspace.Items
                .ShouldNotContain(item, "Item should be removed from workspace after HideCommand is executed.");
        }

        [Fact]
        public void OnTabHideRequested_MovesItemFromSecondaryWorkspaceToHiddenItems()
        {
            DockItemViewModel item = new()
            {
                Id = "hide2",
                Title = "Hideable Secondary",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager secondaryWorkspace = new(split);
            DockControlManager manager = new(new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Horizontal)));
            manager.WindowManager = new WindowManager(new TestWindowFactory());

            manager.AttachSecondaryWorkspace(secondaryWorkspace, null, new Size(300, 200));

            secondaryWorkspace.Items
                .ShouldContain(item, "Sanity: Item should be present in SecondaryWorkspace before HideCommand is executed.");

            manager.HiddenItems
                .ShouldNotContain(item, "Sanity: Item should not be in HiddenItems before HideCommand is executed.");

            item.HideCommand.Execute(null);

            manager.HiddenItems
                .ShouldContain(item, "Item should be added to HiddenItems after HideCommand is executed.");

            secondaryWorkspace.Items
                .ShouldNotContain(item, "Item should be removed from SecondaryWorkspace after HideCommand is executed.");
        }

        [Fact]
        public void OnTabHideRequested_SetsPreferredWorkspaceId()
        {
            DockItemViewModel item = new()
            {
                Id = "hide1",
                Title = "Hideable",
                Context = new Object(),
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockWorkspaceManager workspace = new(DockTree.Horizontal(tabNode));
            DockControlManager manager = new(workspace);
            _ = manager;

            item.HideCommand.Execute(null);

            String? preferredWorkspaceId = DockContext.GetPreferredWorkspaceId(item);

            preferredWorkspaceId
                .ShouldNotBeNull("Preferred workspace ID should be set if the item is hidden.");

            preferredWorkspaceId
                .ShouldBe(workspace.Id, "Preferred workspace ID should be set to the correct value.");
        }

        [AvaloniaFact]
        public void OnTabMinimizeRequested_ForLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple tab nodes
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            DockControlManager manager = new(new DockWorkspaceManager(initialTree));

            // Act: minimize all items in the primary workspace
            foreach (DockTabNodeViewModel tabNode in initialTree.Children.OfType<DockTabNodeViewModel>().ToList())
            {
                foreach (DockItemViewModel item in tabNode.Tabs.ToList())
                {
                    item.MinimizeCommand.Execute(null);
                }
            }

            // Assert: PrimaryWorkspace still has a valid DockTabNodeViewModel
            DockNodeViewModel root = manager.PrimaryWorkspace.DockTree;
            root
                .ShouldBeOfType<DockSplitNodeViewModel>("PrimaryWorkspace root should be a split node.");

            DockSplitNodeViewModel split = (DockSplitNodeViewModel)root;
            split.Children.Count
                .ShouldBe(1, "PrimaryWorkspace should contain one fallback tab node.");

            split.Children[0]
                .ShouldBeOfType<DockTabNodeViewModel>("Fallback node should be a DockTabNodeViewModel.");
        }

        [Fact]
        public void OnTabShowRequested_ClearsPeferredWorkspaceId()
        {
            DockItemViewModel item = new()
            {
                Id = "show2",
                Title = "Showable Secondary",
                Context = new Object(),
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockWorkspaceManager secondaryWorkspace = new(split);
            DockControlManager manager = new(new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Horizontal)));
            manager.WindowManager = new WindowManager(new TestWindowFactory());

            manager.AttachSecondaryWorkspace(secondaryWorkspace, null, new Size(300, 200));

            DockContext.SetPreferredWorkspaceId(item, secondaryWorkspace.Id);
            manager.AddHiddenItem(item);

            item.ShowCommand.Execute(null);

            DockContext.GetPreferredWorkspaceId(item)
                .ShouldBeNull("The preferred workspace ID should be cleared after showing the item.");
        }

        [Fact]
        public void OnTabShowRequested_MovesItemBackToPrimaryWorkspace()
        {
            DockItemViewModel item = new()
            {
                Id = "show1",
                Title = "Showable",
                Context = new Object(),
            };

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            DockControlManager manager = new(workspace);
            manager.AddHiddenItem(item);

            manager.HiddenItems
                .ShouldContain(item, "Sanity: Item should be in HiddenItems before ShowCommand is executed.");

            workspace.Items
                .ShouldNotContain(item, "Sanity: Item should not be in workspace before ShowCommand is executed.");

            item.ShowCommand.Execute(null);

            manager.HiddenItems
                .ShouldNotContain(item, "Item should be removed from HiddenItems after ShowCommand is executed.");

            workspace.Items
                .ShouldContain(item, "Item should be added back to workspace after ShowCommand is executed.");
        }

        [Fact]
        public void OnTabShowRequested_MovesItemBackToSecondaryWorkspace()
        {
            DockItemViewModel item = new()
            {
                Id = "show2",
                Title = "Showable Secondary",
                Context = new Object(),
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockWorkspaceManager secondaryWorkspace = new(split);
            DockControlManager manager = new(new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Horizontal)));
            manager.WindowManager = new WindowManager(new TestWindowFactory());

            manager.AttachSecondaryWorkspace(secondaryWorkspace, null, new Size(300, 200));

            DockContext.SetPreferredWorkspaceId(item, secondaryWorkspace.Id);
            manager.AddHiddenItem(item);

            manager.HiddenItems
                .ShouldContain(item, "Sanity: Item should be in HiddenItems before ShowCommand is executed.");

            secondaryWorkspace.Items
                .ShouldNotContain(item, "Sanity: Item should not be in SecondaryWorkspace before ShowCommand is executed.");

            item.ShowCommand.Execute(null);

            manager.HiddenItems
                .ShouldNotContain(item, "Item should be removed from HiddenItems after ShowCommand is executed.");

            secondaryWorkspace.Items
                .ShouldContain(item, "Item should be added back to SecondaryWorkspace after ShowCommand is executed.");
        }

        /// <summary>
        /// Helper for validating that an intial <see cref="DockTree"/> plus move results in the expected <see cref="DockTree"/>.
        /// </summary>
        private sealed class DockMutationTestBuilder
        {
            private DockNodeViewModel? expectedTree;
            private DockItemViewModel? itemToMove;
            private DockControlManager? manager;
            private DockItemMoveOptions? moveOptions;
            private DockNodeViewModel? targetNode;

            /// <summary>Validates the the current <see cref="DockTree"/> matches the expected <see cref="DockTree"/>.</summary>
            public void Assert(Boolean displayActualTree = false)
            {
                this.manager.ShouldNotBeNull("Initial tree must be set");
                this.itemToMove.ShouldNotBeNull("Item to move must be set");
                this.targetNode.ShouldNotBeNull("Target node must be set");
                this.moveOptions.ShouldNotBeNull("Move options must be set");
                this.expectedTree.ShouldNotBeNull("Expected tree must be set");

                Boolean result = this.manager!.MoveItem(this.itemToMove!, this.targetNode!, this.moveOptions!);
                Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                result.ShouldBeTrue("Move operation should succeed");

                DockNodeViewModel actual = this.manager!.PrimaryWorkspace.DockTree;

                if (displayActualTree)
                {
                    DockTree.DisplayTree(actual);
                }

                AssertTreesEqual(actual, this.expectedTree!);
            }

            /// <summary>Define the expected <see cref="DockTree"/>.</summary>
            public DockMutationTestBuilder ExpectTree(DockNodeViewModel expected)
            {
                this.expectedTree = expected;
                return this;
            }

            /// <summary>Define the initial <see cref="DockTree"/>.</summary>
            public DockMutationTestBuilder WithInitialTree(DockSplitNodeViewModel root)
            {
                this.manager = new DockControlManager(new DockWorkspaceManager(root));
                return this;
            }

            /// <summary>Define the move operation to be made.</summary>
            public DockMutationTestBuilder WithMoveItem(DockItemViewModel item, DockNodeViewModel target, DropZone zone)
            {
                this.itemToMove = item;
                this.targetNode = target;
                this.moveOptions = new DockItemMoveOptions { DropZone = zone };
                return this;
            }

            private static void AssertTreesEqual(DockNodeViewModel actual, DockNodeViewModel expected, String path = "root")
            {
                actual.ShouldNotBeNull($"Node at '{path}' should not be null.");
                expected.ShouldNotBeNull($"Expected node at '{path}' should not be null.");

                actual.GetType().ShouldBe(expected.GetType(), $"Node type mismatch at '{path}'.");

                if (actual is DockTabNodeViewModel actualTab && expected is DockTabNodeViewModel expectedTab)
                {
                    actualTab.Tabs.Select(t => t.Id)
                        .ShouldBe(expectedTab.Tabs.Select(t => t.Id), $"Tab contents mismatch at '{path}'.");
                }
                else if (actual is DockSplitNodeViewModel actualSplit && expected is DockSplitNodeViewModel expectedSplit)
                {
                    actualSplit.Orientation
                        .ShouldBe(expectedSplit.Orientation, $"Split orientation mismatch at '{path}'.");

                    actualSplit.Children.Count
                        .ShouldBe(expectedSplit.Children.Count, $"Child count mismatch at '{path}'.");

                    for (Int32 i = 0; i < actualSplit.Children.Count; i++)
                    {
                        String childPath = $"{path}[{i}]";
                        AssertTreesEqual(actualSplit.Children[i], expectedSplit.Children[i], childPath);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected node type mismatch at '{path}'.");
                }
            }
        }
    }
}
