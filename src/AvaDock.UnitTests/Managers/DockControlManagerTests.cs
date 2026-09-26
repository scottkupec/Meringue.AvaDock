// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Specialized;
using Avalonia.Layout;
using Meringue.AvaDock.Events;
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

            found.ShouldBe(item, "FindItem should return the item from the primary workspace.");
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

            found.ShouldBe(item, "FindItem should locate a hidden item via HiddenItems enumeration.");
        }

        [Fact]
        public void FindItem_ReturnsNullWhenNotFound()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab("anytab"));
            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockItemViewModel? found = manager.FindItem("does-not-exist");

            found.ShouldBeNull("FindItem should return null when the item id is not found in primary, secondary or hidden collections.");
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

            found.ShouldBe(tab, "FindNode should return the node from the primary workspace.");
        }

        [Fact]
        public void FindNode_ReturnsNullWhenNotFound()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(split);
            DockControlManager manager = new(workspace);

            DockNodeViewModel? found = manager.FindNode("does-not-exist");

            found.ShouldBeNull("FindNode should return null when the node id is not found in primary or secondary workspaces.");
        }

        [Fact]
        public void AttachSecondaryWorkspace_CancelsWhenWorkspaceAttachingCancels()
        {
            DockSplitNodeViewModel primaryRoot = DockTree.Horizontal(DockTree.Tab("primary"));
            DockWorkspaceManager primary = new(primaryRoot);
            CancelAttachingDockControlManager manager = new(primary);
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            manager.TriggerWorkspaceAttaching(secondary);

            manager.AttachingWasCancelled.ShouldBeTrue("Workspace attaching cancellation should be handled.");
            manager.SecondaryWorkspaces.ShouldBeEmpty("A secondary workspace should not have been attached.");
        }

        [Fact]
        public void EventAccessors_CanSubscribeAndUnsubscribe()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);

            static void Handler(
                Object? o,
                NotifyCollectionChangedEventArgs eventArgs)
            {
            }

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
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;

            workspace.RaiseItemClosed(item);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is closed.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemHidden()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;

            workspace.RaiseItemHidden(item);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is hidden.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemMinimized()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;

            workspace.RaiseItemMinimized(item);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is minimized.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemMoved()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            DockTabNodeViewModel source = new();
            DockTabNodeViewModel target = new();

            workspace.RaiseItemMoved(item, source, target, MovePlacement.On, null);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is moved.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemRestored()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;

            workspace.RaiseItemRestored(item);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is restored.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenItemShown()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;

            workspace.RaiseItemShown(item);

            raised.ShouldBeTrue("LayoutChanged should be raised when item is shown.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenWorkspaceAttached()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            manager.TriggerWorkspaceAttached(secondary);

            raised.ShouldBeTrue("LayoutChanged should be raised when workspace is attached.");
        }

        [Fact]
        public void LayoutChangedRaised_WhenWorkspaceDetached()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            manager.LayoutChanged += (_, _) => raised = true;
            DockSplitNodeViewModel secondaryRoot = new(Orientation.Horizontal);
            DockWorkspaceManager secondary = new(secondaryRoot);

            manager.TriggerWorkspaceDetached(secondary);

            raised.ShouldBeTrue("LayoutChanged should be raised when workspace is detached.");
        }

        [Fact]
        public void EventAccessors_MultipleSubscribersReceiveEvents()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            Int32 callCount = 0;
            manager.ItemClosed += (_, _) => callCount++;
            manager.ItemClosed += (_, _) => callCount++;

            workspace.RaiseItemClosed(item);

            callCount.ShouldBe(2, "Multiple subscribers should each receive the event.");
        }

        [Fact]
        public void EventForwarded_WhenItemClosed()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            manager.ItemClosed += (_, _) => raised = true;

            workspace.RaiseItemClosed(item);

            raised.ShouldBeTrue("ItemClosed should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemHidden()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            manager.ItemHidden += (_, _) => raised = true;

            workspace.RaiseItemHidden(item);

            raised.ShouldBeTrue("ItemHidden should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemMinimized()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            manager.ItemMinimized += (_, _) => raised = true;

            workspace.RaiseItemMinimized(item);

            raised.ShouldBeTrue("ItemMinimized should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemMoved()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            DockTabNodeViewModel source = new();
            DockTabNodeViewModel target = new();
            manager.ItemMoved += (_, _) => raised = true;

            workspace.RaiseItemMoved(item, source, target, MovePlacement.On, null);

            raised.ShouldBeTrue("ItemMoved should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemRestored()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            manager.ItemRestored += (_, _) => raised = true;

            workspace.RaiseItemRestored(item);

            raised.ShouldBeTrue("ItemRestored should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void EventForwarded_WhenItemShown()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            TestWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            Boolean raised = false;
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            manager.ItemShown += (_, _) => raised = true;

            workspace.RaiseItemShown(item);

            raised.ShouldBeTrue("ItemShown should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void HiddenItems_PropertyChanged_RaisedWhenChanged()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            String? changedProperty = null;
            manager.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            item.HideCommand.Execute(null);

            changedProperty.ShouldBe(nameof(DockControlManager.HiddenItems), "PropertyChanged should be raised for HiddenItems.");
        }

        [Fact]
        public void HiddenItemsChanged_ForwardedFromPrimaryWorkspace()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            Boolean raised = false;
            manager.HiddenItemsChanged += (_, _) => raised = true;

            item.HideCommand.Execute(null);

            raised.ShouldBeTrue("HiddenItemsChanged should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void Items_PropertyChanged_RaisedWhenChanged()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            String? changedProperty = null;
            manager.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            item.CloseCommand.Execute(null);

            changedProperty.ShouldBe(nameof(DockControlManager.Items), "PropertyChanged should be raised for Items.");
        }

        [Fact]
        public void ItemsChanged_ForwardedFromPrimaryWorkspace()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            Boolean raised = false;
            manager.ItemsChanged += (_, _) => raised = true;

            item.CloseCommand.Execute(null);

            raised.ShouldBeTrue("ItemsChanged should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void MinimizedItems_PropertyChanged_RaisedWhenChanged()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            String? changedProperty = null;
            manager.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

            item.MinimizeCommand.Execute(null);

            changedProperty.ShouldBe(nameof(DockControlManager.MinimizedItems), "PropertyChanged should be raised for MinimizedItems.");
        }

        [Fact]
        public void MinimizedItemsChanged_ForwardedFromPrimaryWorkspace()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager workspace = new(root);
            DockControlManager manager = new(workspace);
            DockItemViewModel item = root.FindDescendentItem<DockItemViewModel>("item1")!;
            Boolean raised = false;
            manager.MinimizedItemsChanged += (_, _) => raised = true;

            item.MinimizeCommand.Execute(null);

            raised.ShouldBeTrue("MinimizedItemsChanged should be forwarded from workspace to control manager.");
        }

        [Fact]
        public void RemoveFloatingWorkspace_MigratesItemsHiddenMinimized()
        {
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

            manager.RemoveFloatingWorkspace(secondaryWorkspace);

            manager.PrimaryWorkspace.Items.ShouldContain(item, "Item should be migrated to primary workspace.");
            manager.PrimaryWorkspace.HiddenItems.ShouldContain(hiddenItem, "Hidden item should be migrated to primary workspace.");
            manager.PrimaryWorkspace.MinimizedItems.ShouldContain(minimizedItem, "Minimized item should be migrated to primary workspace.");
        }

        [Fact]
        public void WorkspaceAttachedEvent_RaisedWhenWorkspaceAttached()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            DockWorkspaceManager secondary = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            manager.WorkspaceAttached += (_, _) => raised = true;

            manager.TriggerWorkspaceAttached(secondary);

            raised.ShouldBeTrue("WorkspaceAttached should be raised when workspace is attached.");
        }

        [Fact]
        public void WorkspaceDetachedEvent_RaisedWhenWorkspaceDetached()
        {
            DockSplitNodeViewModel root = DockTree.Horizontal(DockTree.Tab("item1"));
            DockWorkspaceManager primary = new(root);
            TestDockControlManager manager = new(primary);
            Boolean raised = false;
            DockWorkspaceManager secondary = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            manager.WorkspaceDetached += (_, _) => raised = true;

            manager.TriggerWorkspaceDetached(secondary);

            raised.ShouldBeTrue("WorkspaceDetached should be raised when workspace is detached.");
        }

        /// <summary>
        /// IoC class so we can raise <see cref="DockWorkspaceManager"/> events directly.
        /// </summary>
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

        /// <summary>
        /// IoC class so we can raise <see cref="DockControlManager"/> events directly.
        /// </summary>
        private sealed class TestDockControlManager : DockControlManager
        {
            public TestDockControlManager(DockWorkspaceManager root)
                : base(root)
            {
            }

            public void TriggerWorkspaceAttached(DockWorkspaceManager workspace) => this.OnWorkspaceAttached(new DockWorkspaceAttachedEventArgs(workspace));

            public void TriggerWorkspaceDetached(DockWorkspaceManager workspace) => this.OnWorkspaceDetached(new DockWorkspaceDetachedEventArgs(workspace));
        }

        /// <summary>
        /// IoC class so we can test that <see cref="DockControlManager"/> cancelling events works correctly.
        /// </summary>
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
