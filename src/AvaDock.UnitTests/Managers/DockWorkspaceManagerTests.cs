// Copyright (C) Scott Kupec. All rights reserved.

using System;
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

            insertedTabNode!.Tabs
                .ShouldContain(item, $"New {nameof(DockTabNodeViewModel)} should contain the inserted tab.");
        }

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
        public void AddItem_ShouldInsertItembWhenNoPreferredPanelIsNotPresent()
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

        [Fact]
        public void MinimizeTab_ShouldMoveTabToMinimizedTabs()
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
        public void MinimizeTab_ShouldSetPreferredTabPanelId()
        {
            DockItemViewModel item = new() { Title = "Test Item" };
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockTabNodeViewModel tabNode = new();
            split.AddChild(tabNode);
            tabNode.AddTab(item);

            DockWorkspaceManager manager = new(split);
            _ = manager;

            item.MinimizeCommand.Execute(null);

            String? preferredPanelId = DockContext.GetPreferredTabPanelId(item);

            preferredPanelId
                .ShouldNotBeNull("Minimizing a tab should set the preferred tab panel id.");

            preferredPanelId
                .ShouldBe(tabNode.Id, "Minimizing a tab should set the correct preferred tab panel id.");
        }

        [Fact]
        public void RemoveItem_ShouldClearWorkspace()
        {
            DockItemViewModel item = new() { Title = "Item Without Preference" };
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockTabNodeViewModel tabNode = new();
            split.AddChild(tabNode);
            DockWorkspaceManager manager = new(split);

            Boolean result = manager.AddItem(item);

            result
                .ShouldBeTrue($"Sanity: {nameof(DockWorkspaceManager.AddItem)} should succeed.");

            DockContext.GetWorkspace(item)
                .ShouldBe(manager, "Sanity: Workspace should be set.");

            result = manager.RemoveItem(item);

            result
                .ShouldBeTrue($"{nameof(DockWorkspaceManager.RemoveItem)} should succeed.");

            DockContext.GetWorkspace(item)
                .ShouldBeNull("Workspace should be cleared.");
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
        public void RestoreTab_ShouldClearPanelId()
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
        public void RestoreTab_ShouldMoveTabBackToDockTree()
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
    }
}
