// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
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
    public class DockWorkspaceManagerTests
    {
        [Fact]
        public void AddItem_ShouldInsertItemUsingPreferredPanelId()
        {
            DockItemViewModel item = new() { Title = "Preferred Tab" };
            DockTabNodeViewModel preferredNode = DockTree.Tab();
            DockWorkspaceManager manager = new(DockTree.Horizontal(preferredNode));
            DockContext.SetPreferredTabPanelId(item, preferredNode.Id);

            Boolean result = manager.AddItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.AddItem)} should succeed when preferred panel ID is set.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBe(preferredNode, $"Item should be inserted into preferred {nameof(DockTabNodeViewModel)}.");

            DockContext.GetPreferredTabPanelId(item)
                .ShouldBeNull("PreferredTabPanelId should be cleared after insertion.");
        }

        [Fact]
        public void AddItem_ShouldInsertItembWhenNoPreferredPanelId()
        {
            // Arrange
            DockItemViewModel item = new() { Title = "Tab Without Preference" };
            DockWorkspaceManager manager = new(DockTree.Horizontal(DockTree.Tab()));

            // Act
            Boolean result = manager.AddItem(item);

            // Assert
            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.AddItem)} should succeed when no preferred panel ID is set.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldNotBeNull($"Item should be inserted into {nameof(DockWorkspaceManager.DockTree)}.");

            DockContext.GetPreferredTabPanelId(item)
                .ShouldBeNull("PreferredTabPanelId should be cleared after insertion.");
        }

        [Fact]
        public void AddItem_ShouldInsertItembWhenPreferredPanelIsNotPresent()
        {
            DockItemViewModel item = new() { Title = "Tab With Missing Preference" };
            DockWorkspaceManager manager = new(DockTree.Horizontal(DockTree.Tab()));
            DockContext.SetPreferredTabPanelId(item, "not-present-tab-node");

            Boolean result = manager.AddItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.AddItem)} should succeed when no preferred panel ID is set.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldNotBeNull($"Item should be inserted into {nameof(DockWorkspaceManager.DockTree)}.");

            DockContext.GetPreferredTabPanelId(item)
                .ShouldBeNull("PreferredTabPanelId should be cleared after insertion.");
        }

        [Fact]
        public void AddItem_ShouldInsertSplitNodeWhenDockTreeHasNoTabNode()
        {
            DockItemViewModel item = new() { Title = "Fallback Tab Item" };
            DockWorkspaceManager manager = new(DockTree.Horizontal());

            DockTabNodeViewModel? firstTabNode = manager.DockTree.FindFirstTabNode();
            firstTabNode
                .ShouldBeNull($"Sanity: Initial {nameof(DockWorkspaceManager.DockTree)} should not contain any {nameof(DockTabNodeViewModel)}.");

            Boolean result = manager.AddItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.AddItem)} should succeed even when no tab node exists.");

            DockTabNodeViewModel? insertedTabNode = manager.DockTree.FindOwningTabNode(item.Id);

            insertedTabNode
                .ShouldNotBeNull($"{nameof(DockWorkspaceManager.DockTree)} should contain a new {nameof(DockTabNodeViewModel)}.");

            insertedTabNode.Tabs
                .ShouldContain(item, $"New {nameof(DockTabNodeViewModel)} should contain the inserted tab.");
        }

        [Fact]
        public void AddItem_ShouldSetWorkspace()
        {
            DockItemViewModel item = new() { Title = "Item Without Preference" };
            DockWorkspaceManager manager = new(DockTree.Horizontal(DockTree.Tab()));

            Boolean result = manager.AddItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.AddItem)} should succeed.");

            DockContext.GetWorkspace(item)
                .ShouldBe(manager, "Workspace should be set.");
        }

        [AvaloniaFact]
        public void CloseItem_ForLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple tab nodes
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            // Act: hide all items in the primary workspace
            foreach (DockTabNodeViewModel tabNode in initialTree.Children.OfType<DockTabNodeViewModel>().ToList())
            {
                foreach (DockItemViewModel item in tabNode.Tabs.ToList())
                {
                    item.CloseCommand.Execute(null);
                }
            }

            initialTree.Children.Count
                .ShouldBe(1, "Workspace should contain one fallback tab node.");

            initialTree.Children[0]
                .ShouldBeOfType<DockTabNodeViewModel>("Fallback node should be a DockTabNodeViewModel.");
        }

        [Fact]
        public void CloseItem_RemovesItemFromHiddenItems()
        {
            DockItemViewModel item = new()
            {
                Id = "close1",
                Title = "Closable",
                Context = new Object(),
            };

            DockContext.SetItemState(item, DockItemState.Hidden);

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            workspace.AddItem(item);

            workspace.HiddenItems
                .ShouldContain(item, $"Item should be in {nameof(workspace.HiddenItems)} before CloseCommand is executed.");

            item.CloseCommand.Execute(null);

            workspace.HiddenItems
                .ShouldNotContain(item, $"Item should be removed from {nameof(workspace.HiddenItems)} after CloseCommand is executed.");
        }

        [Fact]
        public void CloseItem_RemovesItemFromItems()
        {
            DockItemViewModel item = new()
            {
                Id = "close1",
                Title = "Closable",
                Context = new Object(),
            };

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            workspace.AddItem(item);

            workspace.Items
                .ShouldContain(item, $"Item should be in {nameof(workspace.Items)} before CloseCommand is executed.");

            item.CloseCommand.Execute(null);

            workspace.Items
                .ShouldNotContain(item, $"Item should be removed from {nameof(workspace.Items)} after CloseCommand is executed.");
        }

        [Fact]
        public void CloseItem_RemovesItemFromMinimizedItems()
        {
            DockItemViewModel item = new()
            {
                Id = "close1",
                Title = "Closable",
                Context = new Object(),
            };

            DockContext.SetItemState(item, DockItemState.Minimized);

            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            workspace.AddItem(item);

            workspace.MinimizedItems
                .ShouldContain(item, $"Item should be in {nameof(workspace.MinimizedItems)} before CloseCommand is executed.");

            item.CloseCommand.Execute(null);

            workspace.HiddenItems
                .ShouldNotContain(item, $"Item should be removed from {nameof(workspace.MinimizedItems)} after CloseCommand is executed.");
        }

        [AvaloniaFact]
        public void HideItem_ForLastItemLeavesValidDropTarget()
        {
            // Arrange: initial tree with multiple tab nodes
            DockSplitNodeViewModel initialTree = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            // Act: hide all items in the primary workspace
            foreach (DockTabNodeViewModel tabNode in initialTree.Children.OfType<DockTabNodeViewModel>().ToList())
            {
                foreach (DockItemViewModel item in tabNode.Tabs.ToList())
                {
                    item.HideCommand.Execute(null);
                }
            }

            initialTree.Children.Count
                .ShouldBe(1, "Workspace should contain one fallback tab node.");

            initialTree.Children[0]
                .ShouldBeOfType<DockTabNodeViewModel>("Fallback node should be a DockTabNodeViewModel.");
        }

        [Fact]
        public void HideItem_MovesItemFromDockTreeToHiddenItems()
        {
            DockItemViewModel item = new() { Title = "Test Item" };
            DockTabNodeViewModel tabNode = new();
            DockContext.SetItemState(item, DockItemState.Normal);

            tabNode.AddTab(item);

            DockWorkspaceManager manager = new(DockTree.Horizontal(tabNode));
            manager.AddItem(item);

            manager.HiddenItems
                .ShouldNotContain(item, $"Item should be in not be {nameof(DockWorkspaceManager.HiddenItems)} initially.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldNotBeNull($"Item be in {nameof(DockWorkspaceManager.HiddenItems)} initially.");

            item.HideCommand.Execute(null);

            manager.HiddenItems
                .ShouldContain(item, $"Item should be in {nameof(DockWorkspaceManager.HiddenItems)} after minimize command.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBeNull($"Item should no longer be in {nameof(DockWorkspaceManager.HiddenItems)} after minimization.");
        }

        [Fact]
        public void ItemClosing_CancelPreventsClose()
        {
            DockItemViewModel item = new() { Title = "Closeable" };
            DockWorkspaceManager workspace = new(DockTree.Horizontal(DockTree.Tab()));
            workspace.AddItem(item);

            Boolean closedRaised = false;
            workspace.ItemClosing += (_, e) => e.Cancel = true;
            workspace.ItemClosed += (_, _) => closedRaised = true;

            item.CloseCommand.Execute(null);

            workspace.Items.ShouldContain(item, "Item should remain in workspace when closing is cancelled.");
            closedRaised.ShouldBeFalse("ItemClosed should not be raised when closing is cancelled.");
        }

        [Fact]
        public void ItemHiding_CancelPreventsHide()
        {
            DockItemViewModel item = new() { Title = "Hideable" };
            DockWorkspaceManager workspace = new(DockTree.Horizontal(DockTree.Tab()));
            workspace.AddItem(item);

            Boolean hiddenRaised = false;
            workspace.ItemHiding += (_, e) => e.Cancel = true;
            workspace.ItemHidden += (_, _) => hiddenRaised = true;

            item.HideCommand.Execute(null);

            workspace.Items.ShouldContain(item, "Item should remain in workspace when hiding is cancelled.");
            workspace.HiddenItems.ShouldNotContain(item, "Item should not be moved to hidden items when hiding is cancelled.");
            hiddenRaised.ShouldBeFalse("ItemHidden should not be raised when hiding is cancelled.");
        }

        [Fact]
        public void ItemMinimizing_CancelPreventsMinimize()
        {
            DockItemViewModel item = new() { Title = "Minimizable" };
            DockWorkspaceManager workspace = new(DockTree.Horizontal(DockTree.Tab()));
            workspace.AddItem(item);

            Boolean minimizedRaised = false;
            workspace.ItemMinimizing += (_, e) => e.Cancel = true;
            workspace.ItemMinimized += (_, _) => minimizedRaised = true;

            item.MinimizeCommand.Execute(null);

            workspace.Items.ShouldContain(item, "Item should remain in workspace when minimizing is cancelled.");
            workspace.MinimizedItems.ShouldNotContain(item, "Item should not be moved to minimized items when minimizing is cancelled.");
            minimizedRaised.ShouldBeFalse("ItemMinimized should not be raised when minimizing is cancelled.");
        }

        [Fact]
        public void MinimizeItem_ShouldMoveTabToMinimizedTabs()
        {
            DockItemViewModel item = new() { Title = "Test Item" };
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockWorkspaceManager manager = new(DockTree.Horizontal(tabNode));

            item.MinimizeCommand.Execute(null);

            manager.MinimizedItems
                .ShouldContain(item, $"Item should be in {nameof(DockWorkspaceManager.MinimizedItems)} after minimize command.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBeNull("Item should no longer be in {nameof(DockWorkspaceManager.MinimizedItems)} after minimization.");

            manager.ShouldShowMinimizedItems
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.ShouldShowMinimizedItems)} should be true when an item is minimized.");
        }

        [Fact]
        public void MinimizeItem_ShouldSetPreferredTabPanelId()
        {
            DockItemViewModel item = new() { Title = "Test Item" };
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockTabNodeViewModel tabNode = new();
            split.AddChild(tabNode);
            tabNode.AddTab(item);

            _ = new DockWorkspaceManager(split);

            item.MinimizeCommand.Execute(null);

            String? preferredPanelId = DockContext.GetPreferredTabPanelId(item);

            preferredPanelId
                .ShouldNotBeNull("Minimizing a tab should set the preferred tab panel id.");

            preferredPanelId
                .ShouldBe(tabNode.Id, "Minimizing a tab should set the correct preferred tab panel id.");
        }

        [Fact]
        public void RemoveItem_ShouldRemoveTabNodeOnLastItemRemoved()
        {
            DockItemViewModel item = new() { Title = "Removable Item" };
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockTabNodeViewModel tabNode = new();
            split.AddChild(tabNode);
            tabNode.AddTab(item);
            DockWorkspaceManager manager = new(split);

            Boolean result = manager.RemoveItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.RemoveItem)} should succeed when item is present in {nameof(DockWorkspaceManager.DockTree)}.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBeNull($"Item should be removed from {nameof(DockWorkspaceManager.DockTree)}.");

            manager.DockTree.FindNode(tabNode.Id)
                .ShouldBeNull("Removing the last item from a tab node should remove the entire tab node.");
        }

        [Fact]
        public void RemoveItem_ShouldRemoveTabWhenPresentInDockTree()
        {
            DockItemViewModel item = new() { Title = "Removable Item" };
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockItemViewModel stayingTab = new() { Title = "Removable Item" };
            tabNode.AddTab(stayingTab);

            DockWorkspaceManager manager = new(DockTree.Horizontal(tabNode));

            Boolean result = manager.RemoveItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.RemoveItem)} should succeed when item is present.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBeNull($"Item should be removed from {nameof(DockWorkspaceManager.DockTree)}.");

            manager.DockTree.FindNode(stayingTab.Id)
                .ShouldBeNull("The item not removed should still be present.");
        }

        [Fact]
        public void RemoveItem_ShouldReturnTrueWhenTabNotPresent()
        {
            DockItemViewModel item = new() { Title = "Missing Item" };
            DockWorkspaceManager manager = new(DockTree.Horizontal(DockTree.Tab()));

            Boolean result = manager.RemoveItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.RemoveItem)} should return true even if item is not present.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldBeNull($"Item should not exist in {nameof(DockWorkspaceManager.DockTree)}.");
        }

        [Fact]
        public void RestoreItem_ShouldClearPanelId()
        {
            DockItemViewModel item = new() { Title = "Restorable Item" };
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            _ = new DockWorkspaceManager(DockTree.Horizontal(tabNode));
            item.MinimizeCommand.Execute(null);

            item.RestoreCommand.Execute(null);

            DockContext.GetPreferredTabPanelId(item)
                .ShouldBeNull("Restoring a tab should clear the preferred tab panel id.");
        }

        [Fact]
        public void RestoreItem_ShouldMoveTabBackToDockTree()
        {
            DockItemViewModel item = new() { Title = "Restorable Item" };
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockWorkspaceManager manager = new(DockTree.Horizontal(tabNode));

            item.MinimizeCommand.Execute(null);

            manager.MinimizedItems
                .ShouldContain(item, "Sanity: Item should be minimized before restore.");

            item.RestoreCommand.Execute(null);

            manager.MinimizedItems
                .ShouldNotContain(item, $"Item should be removed from {nameof(DockWorkspaceManager.MinimizedItems)} after restore.");

            manager.DockTree.FindOwningTabNode(item.Id)
                .ShouldNotBeNull($"Item should be reinserted into {nameof(DockWorkspaceManager.DockTree)} after restore.");

            manager.ShouldShowMinimizedItems
                .ShouldBeFalse($"{nameof(DockWorkspaceManager.ShouldShowMinimizedItems)} should be false after restoring the only minimized item.");
        }

        [Fact]
        public void ShowItem_MovesItemBackToDockTree()
        {
            DockItemViewModel item = new()
            {
                Id = "show2",
                Title = "Showable Secondary",
                Context = new Object(),
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockWorkspaceManager workspace = new(split);
            DockContext.SetItemState(item, DockItemState.Hidden);
            workspace.AddItem(item);

            workspace.HiddenItems
                .ShouldContain(item, "Sanity: Item should be in HiddenItems before ShowCommand is executed.");

            workspace.Items
                .ShouldNotContain(item, "Sanity: Item should not be in workspace before ShowCommand is executed.");

            item.ShowCommand.Execute(null);

            workspace.HiddenItems
                .ShouldNotContain(item, "Item should be removed from HiddenItems after ShowCommand is executed.");

            workspace.Items
                .ShouldContain(item, "Item should be added back to workspace after ShowCommand is executed.");
        }
    }
}
