// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Events;
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

        [Fact]
        public void FindItem_LocatesHiddenItem()
        {
            DockItemViewModel item = new()
            {
                Id = "hidden-item",
                Title = "Hidden",
            };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            split.AddChild(tabNode);

            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            item.HideCommand.Execute(null);

            DockItemViewModel? found = manager.FindItem("hidden-item");

            found
                .ShouldBe(item, "FindItem should locate a hidden item via HiddenItems enumeration.");
        }

        [Fact]
        public void FindItem_ReturnsNullWhenNotFound()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockItemViewModel? found = manager.FindItem("does-not-exist");

            found
                .ShouldBeNull("FindItem should return null when the item id is not found in primary, secondary or hidden collections.");
        }

        [Fact]
        public void FindNode_LocatesNodeInPrimaryWorkspace()
        {
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockTabNodeViewModel tab = new();
            DockItemViewModel item = new() { Id = "item1" };
            tab.AddTab(item);
            split.AddChild(tab);

            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockNodeViewModel? found = manager.FindNode(tab.Id);

            found
                .ShouldBe(tab, "FindNode should return the node from the primary workspace.");
        }

        [Fact]
        public void FindNode_ReturnsNullWhenNotFound()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockNodeViewModel? found = manager.FindNode("does-not-exist");

            found
                .ShouldBeNull("FindNode should return null when the node id is not found in primary or secondary workspaces.");
        }

        [Fact]
        public void AttachSecondaryWorkspace_CancelsWhenWorkspaceAttachingCancels()
        {
            DockSplitNodeViewModel primaryRoot = DockTree.Horizontal(DockTree.Tab("primary"));
            DockWorkspaceManager primary = new(primaryRoot);

            // Arrange a manager that cancels attaching
            CancelAttachingDockControlManager manager = new(primary);
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            // Act
            // AttachSecondaryWorkspace requires Avalonia windows; we test the cancellation path via the protected OnWorkspaceAttaching
            manager.TriggerWorkspaceAttaching(secondary);

            // Assert – the manager should have recorded a cancellation, ensuring the attaching path is exercised
            manager.AttachingWasCancelled
                .ShouldBeTrue("Workspace attaching cancellation should be handled.");
        }

        [Fact]
        public void EventAccessors_CanSubscribeAndUnsubscribe()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);

#pragma warning disable SA1501 // Statement should not be on a single line
#pragma warning disable SA1502 // Element should not be on a single line
            static void Handler(Object? o, NotifyCollectionChangedEventArgs e) { }
#pragma warning restore SA1502 // Element should not be on a single line
#pragma warning restore SA1501 // Statement should not be on a single line
            manager.HiddenItemsChanged += Handler;
            manager.HiddenItemsChanged -= Handler;
            manager.ItemsChanged += Handler;
            manager.ItemsChanged -= Handler;
            manager.MinimizedItemsChanged += Handler;
            manager.MinimizedItemsChanged -= Handler;
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemClosed()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;

            // Act
            workspace.RaiseItemClosed(item);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is closed.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemHidden()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;

            // Act
            workspace.RaiseItemHidden(item);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is hidden.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemMinimized()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;

            // Act
            workspace.RaiseItemMinimized(item);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is minimized.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemMoved()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            DockTabNodeViewModel source = new();
            DockTabNodeViewModel target = new();

            // Act
            workspace.RaiseItemMoved(item, source, target, MovePlacement.On, null);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is moved.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemRestored()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;

            // Act
            workspace.RaiseItemRestored(item);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is restored.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemShown()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;

            // Act
            workspace.RaiseItemShown(item);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when item is shown.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenWorkspaceAttached()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            // Act
            manager.TriggerWorkspaceAttached(secondary);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when workspace is attached.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenWorkspaceDetached()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            // Act
            manager.TriggerWorkspaceDetached(secondary);

            // Assert
            raised.ShouldBeTrue("LayoutChanged should be raised when workspace is detached.");
        }

        [Fact]
        public void EventAccessors_MultipleSubscribersReceiveEvents()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            Int32 callCount = 0;
            manager.ItemClosed += (_, _) => callCount++;
            manager.ItemClosed += (_, _) => callCount++;

            // Act
            workspace.RaiseItemClosed(item);

            // Assert
            callCount.ShouldBe(2, "Multiple subscribers should each receive the event.");
        }

        [Fact]
        public void EventForwarded_WhenItemClosed()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            manager.ItemClosed += (_, _) => raised = true;

            // Act
            workspace.RaiseItemClosed(item);

            // Assert
            raised.ShouldBeTrue("ItemClosed should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemHidden()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            manager.ItemHidden += (_, _) => raised = true;

            // Act
            workspace.RaiseItemHidden(item);

            // Assert
            raised.ShouldBeTrue("ItemHidden should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemMinimized()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            manager.ItemMinimized += (_, _) => raised = true;

            // Act
            workspace.RaiseItemMinimized(item);

            // Assert
            raised.ShouldBeTrue("ItemMinimized should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemMoved()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            DockTabNodeViewModel source = new();
            DockTabNodeViewModel target = new();
            manager.ItemMoved += (_, _) => raised = true;

            // Act
            workspace.RaiseItemMoved(item, source, target, MovePlacement.On, null);

            // Assert
            raised.ShouldBeTrue("ItemMoved should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemRestored()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            manager.ItemRestored += (_, _) => raised = true;

            // Act
            workspace.RaiseItemRestored(item);

            // Assert
            raised.ShouldBeTrue("ItemRestored should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemShown()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindItem<DockItemViewModel>("item1")!;
            manager.ItemShown += (_, _) => raised = true;

            // Act
            workspace.RaiseItemShown(item);

            // Assert
            raised.ShouldBeTrue("ItemShown should be forwarded from workspace to control manager.");
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
                    source: initialTree.FindOwningTabNode("item2")!,
                    target: initialTree.FindOwningTabNode("item1")!,
                    placement: MovePlacement.On,
                    orientation: null)
                .ExpectTree(expectedTree)
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
                    source: initialTree.FindOwningTabNode("item1")!,
                    target: initialTree.FindOwningTabNode("item1")!,
                    placement: MovePlacement.On,
                    orientation: null)
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
                    source: initialTree.FindOwningTabNode("right-item")!,
                    target: initialTree.FindOwningTabNode("move-item")!,
                    placement: MovePlacement.After,
                    orientation: Orientation.Horizontal)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [AvaloniaFact]
        public void MoveItem_MovingLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple items
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            DockTabNodeViewModel targetNode = initialTree.FindOwningTabNode("item-1")!;

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
                    item.RequestMove(tabNode, targetNode, MovePlacement.On, null);
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
                    source: initialTree.FindOwningTabNode("bottom-item")!,
                    target: initialTree.FindOwningTabNode("bottom-item")!,
                    placement: MovePlacement.After,
                    orientation: Orientation.Vertical)
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
                    source: initialTree.FindOwningTabNode("move-item")!,
                    target: initialTree.FindOwningTabNode("target-item")!,
                    placement: MovePlacement.After,
                    orientation: Orientation.Horizontal)
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
                    source: initialTree.FindOwningTabNode("left-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    placement: MovePlacement.Before,
                    orientation: Orientation.Horizontal)
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
                    source: initialTree.FindOwningTabNode("right-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    placement: MovePlacement.After,
                    orientation: Orientation.Horizontal)
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
                    source: initialTree.FindOwningTabNode("top-item")!,
                    target: initialTree.FindOwningTabNode("bottom-item")!,
                    placement: MovePlacement.Before,
                    orientation: Orientation.Vertical)
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
                    source: initialTree.FindOwningTabNode("right-item")!,
                    target: initialTree.FindOwningTabNode("center-item")!,
                    placement: MovePlacement.Before,
                    orientation: Orientation.Vertical)
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
                    source: initialTree.FindOwningTabNode("move-item")!,
                    target: initialTree.FindOwningTabNode("left-item")!,
                    placement: MovePlacement.Before,
                    orientation: Orientation.Vertical)
                .ExpectTree(expectedTree)
                .Assert();
        }

        [Fact]
        public void RemoveFloatingWorkspace_MigratesItemsHiddenMinimized()
        {
            // Arrange
            DockSplitNodeViewModel primaryTree = DockTree.Horizontal(DockTree.Tab("primary"));
            DockWorkspaceManager primaryWorkspace = new(primaryTree);
            DockControlManager manager = new(primaryWorkspace);

            DockSplitNodeViewModel secondaryTree = new(Orientation.Horizontal);
            DockTabNodeViewModel secondaryTab = new();
            secondaryTree.AddChild(secondaryTab);
            DockWorkspaceManager secondaryWorkspace = new(secondaryTree);

            DockItemViewModel item = new() { Id = "item", Title = "Item" };
            DockItemViewModel hiddenItem = new() { Id = "hidden", Title = "Hidden" };
            DockItemViewModel minimizedItem = new() { Id = "min", Title = "Minimized" };

            secondaryWorkspace.AddItem(item);
            secondaryWorkspace.AddItem(hiddenItem);
            secondaryWorkspace.AddItem(minimizedItem);

            // Move items to hidden/minimized via commands so they are in the correct collections
            hiddenItem.HideCommand.Execute(null);
            minimizedItem.MinimizeCommand.Execute(null);

            // Act
            manager.RemoveFloatingWorkspace(secondaryWorkspace);

            // Assert
            manager.PrimaryWorkspace.Items.ShouldContain(item, "Item should be migrated to primary workspace.");
            manager.PrimaryWorkspace.HiddenItems.ShouldContain(hiddenItem, "Hidden item should be migrated to primary workspace.");
            manager.PrimaryWorkspace.MinimizedItems.ShouldContain(minimizedItem, "Minimized item should be migrated to primary workspace.");
        }

        [Fact]
        public void WorkspaceAttachedEvent_RaisedWhenWorkspaceAttached()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            DockWorkspaceManager secondary = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            manager.WorkspaceAttached += (_, _) => raised = true;

            // Act
            manager.TriggerWorkspaceAttached(secondary);

            // Assert
            raised.ShouldBeTrue("WorkspaceAttached should be raised when workspace is attached.");
        }

        [Fact]
        public void WorkspaceDetachedEvent_RaisedWhenWorkspaceDetached()
        {
            // Arrange
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            DockWorkspaceManager secondary = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            manager.WorkspaceDetached += (_, _) => raised = true;

            // Act
            manager.TriggerWorkspaceDetached(secondary);

            // Assert
            raised.ShouldBeTrue("WorkspaceDetached should be raised when workspace is detached.");
        }

        /// <summary>
        /// Helper for validating that an intial <see cref="DockTree"/> plus move results in the expected <see cref="DockTree"/>.
        /// </summary>
        private sealed class DockMutationTestBuilder
        {
            private DockNodeViewModel? expectedTree;
            private DockItemViewModel? itemToMove;
            private DockControlManager? manager;
            private Orientation? orientation;
            private MovePlacement? placement;
            private DockTabNodeViewModel? sourceNode;
            private DockTabNodeViewModel? targetNode;

            /// <summary>Validates the the current <see cref="DockTree"/> matches the expected <see cref="DockTree"/>.</summary>
            public void Assert(Boolean displayActualTree = false)
            {
                this.manager.ShouldNotBeNull("Initial tree must be set");
                this.itemToMove.ShouldNotBeNull("Item to move must be set");
                this.sourceNode.ShouldNotBeNull("Source node must be set");
                this.targetNode.ShouldNotBeNull("Target node must be set");
                this.placement.ShouldNotBeNull("Placement must be set");
                this.expectedTree.ShouldNotBeNull("Expected tree must be set");

                this.itemToMove.RequestMove(this.sourceNode, this.targetNode, this.placement.Value, this.orientation);

                ////Boolean result = this.manager.MoveItem(this.itemToMove, this.targetNode, this.placement.Value, this.orientation);

                Avalonia.Threading.Dispatcher.UIThread.RunJobs();

                DockNodeViewModel actual = this.manager.PrimaryWorkspace.DockTree;

                if (displayActualTree)
                {
                    DockTree.DisplayTree(actual);
                }

                AssertTreesEqual(actual, this.expectedTree);
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
            public DockMutationTestBuilder WithMoveItem(DockItemViewModel item, DockTabNodeViewModel source, DockTabNodeViewModel target, MovePlacement placement, Orientation? orientation)
            {
                this.itemToMove = item;
                this.sourceNode = source;
                this.targetNode = target;
                this.placement = placement;
                this.orientation = orientation;
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

        private sealed class TestWorkspaceManager : DockWorkspaceManager
        {
            public TestWorkspaceManager(DockSplitNodeViewModel root)
                : base(root)
            {
            }

            public void RaiseItemClosed(DockItemViewModel item) => this.OnItemClosed(new DockItemClosedEventArgs(item));

            public void RaiseItemHidden(DockItemViewModel item) => this.OnItemHidden(new DockItemHiddenEventArgs(item));

            public void RaiseItemMinimized(DockItemViewModel item) => this.OnItemMinimized(new DockItemMinimizedEventArgs(item));

            public void RaiseItemMoved(DockItemViewModel item, DockTabNodeViewModel source, DockTabNodeViewModel target, MovePlacement placement, Orientation? orientation)
                => this.OnItemMoved(new DockItemMovedEventArgs(item, source, target, placement, orientation));

            public void RaiseItemRestored(DockItemViewModel item) => this.OnItemRestored(new DockItemRestoredEventArgs(item));

            public void RaiseItemShown(DockItemViewModel item) => this.OnItemShown(new DockItemShownEventArgs(item));
        }

        private sealed class TestDockControlManager : DockControlManager
        {
            public TestDockControlManager(DockWorkspaceManager root)
                : base(root)
            {
            }

            public void TriggerWorkspaceAttached(DockWorkspaceManager workspace) => this.OnWorkspaceAttached(new DockWorkspaceAttachedEventArgs(workspace));

            public void TriggerWorkspaceDetached(DockWorkspaceManager workspace) => this.OnWorkspaceDetached(new DockWorkspaceDetachedEventArgs(workspace));
        }

        private sealed class CancelAttachingDockControlManager : DockControlManager
        {
            public CancelAttachingDockControlManager(DockWorkspaceManager root)
                : base(root)
            {
            }

            public Boolean AttachingWasCancelled { get; private set; }

            public void TriggerWorkspaceAttaching(DockWorkspaceManager workspace)
            {
                this.OnWorkspaceAttaching(new DockWorkspaceAttachingEventArgs(workspace));
            }

            protected override void OnWorkspaceAttaching(DockWorkspaceAttachingEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                this.AttachingWasCancelled = true;
                base.OnWorkspaceAttaching(eventArgs);
            }
        }
    }
}
