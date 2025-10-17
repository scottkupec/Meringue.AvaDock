// Copyright (C) Scott Kupec. All rights reserved.

using System.Collections.Generic;
using Avalonia.Layout;
using Meringue.AvaDock.UnitTests;
using Meringue.AvaDock.ViewModels;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Services.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockNodeMonitorTests
    {
        [Fact]
        public void Monitor_HandlesMultipleTrees()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNode1 = DockTree.Tab("item-1");
            DockTabNodeViewModel tabNode2 = DockTree.Tab("item-2");

            vars.Monitor.Monitor(tabNode1);
            vars.Monitor.Monitor(tabNode2);

            vars.Hooked
                .ShouldBeEquivalentTo(
                    new List<DockItemViewModel>() { tabNode1.Tabs[0], tabNode2.Tabs[0] },
                    "Monitoring multiple roots should hook all items in each root.");
        }

        [Fact]
        public void Monitor_HandlesSameRootMonitoredTwice()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNode = DockTree.Tab("item");

            // Monitor the same root twice
            vars.Monitor.Monitor(tabNode);
            vars.Monitor.Monitor(tabNode);

            vars.Hooked.Count
                .ShouldBe(
                    1,
                    "Monitoring the same root multiple times should not call the hook callback more than once per item.");

            vars.Hooked[0]
                .ShouldBe(
                    tabNode.Tabs[0],
                    "The hooked tab should be the original item in the node.");
        }

        [Fact]
        public void Monitor_HooksExistingItems()
        {
            TestVariables vars = new();
            DockTabNodeViewModel tabNode = DockTree.Tab("item-1", "item-2");

            vars.Monitor.Monitor(tabNode);

            vars.Hooked
                .ShouldBeEquivalentTo(
                    new List<DockItemViewModel>() { tabNode.Tabs[0], tabNode.Tabs[1] },
                    $"Monitor should hook all existing tabs in a single {nameof(DockTabNodeViewModel)}.");

            vars.Unhooked
                .ShouldBeEmpty("No tabs should be unhooked immediately after monitoring a node.");
        }

        [Fact]
        public void Monitor_HooksNestedItems()
        {
            TestVariables vars = new();

            DockSplitNodeViewModel splitNode = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("item-1", "item-2"),
                    DockTree.Tab("item-3")),
                DockTree.Tab("item-4"));

            vars.Monitor.Monitor(splitNode);

            vars.Hooked.Count
                .ShouldBe(4, $"Monitoring a {nameof(DockSplitNodeViewModel)} should recursively hook all items.");
        }

        [Fact]
        public void Monitor_HooksNewItems()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNode = DockTree.Tab();
            vars.Monitor.Monitor(tabNode);

            DockItemViewModel newTab = new();
            tabNode.AddTab(newTab);

            vars.Hooked
                .ShouldContain(
                    newTab,
                    $"Adding a new {nameof(DockItemViewModel)} to a monitored {nameof(DockTabNodeViewModel)} should call the hook callback for that item.");
        }

        [Fact]
        public void Monitor_HooksNewTabNodes()
        {
            TestVariables vars = new();

            DockSplitNodeViewModel splitNode = new(Orientation.Horizontal);
            vars.Monitor.Monitor(splitNode);

            DockTabNodeViewModel newTab = DockTree.Tab("item-1", "item-2");

            splitNode.AddChild(newTab);

            vars.Hooked
                .ShouldBeEquivalentTo(
                    new List<DockItemViewModel>() { newTab.Tabs[0], newTab.Tabs[1] },
                    $"Adding a new {nameof(DockTabNodeViewModel)} to a monitored {nameof(DockSplitNodeViewModel)} should hook all tabs in the new node.");
        }

        [Fact]
        public void Monitor_UnhooksAllItemsInRemovedTabNode()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNodeToRemove = DockTree.Tab("item-1", "item-2");

            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab("keep-1"),
                tabNodeToRemove);

            vars.Monitor.Monitor(split);
            split.RemoveChild(tabNodeToRemove);

            vars.Unhooked
                .ShouldBeEquivalentTo(
                    new List<DockItemViewModel>() { tabNodeToRemove.Tabs[0], tabNodeToRemove.Tabs[1] },
                    $"Removing a {nameof(DockTabNodeViewModel)} from a monitored {nameof(DockSplitNodeViewModel)} should call the unhook all items in the node.");
        }

        [Fact]
        public void Monitor_UnhooksRemovedItems()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNode = DockTree.Tab("item-1", "item-2");

            DockItemViewModel itemToRemove = tabNode.Tabs[0];

            vars.Monitor.Monitor(tabNode);
            tabNode.RemoveTab(itemToRemove);

            vars.Unhooked
                .ShouldContain(
                    itemToRemove,
                    $"Removing a {nameof(DockItemViewModel)} from a monitored {nameof(DockTabNodeViewModel)} should call the unhook callback for that item.");
        }

        [Fact]
        public void Unmonitor_UnhooksAllTabItems()
        {
            TestVariables vars = new();

            DockTabNodeViewModel tabNode = DockTree.Tab("item-1", "item-2");

            vars.Monitor.Monitor(tabNode);
            vars.Monitor.Unmonitor(tabNode);

            vars.Unhooked
                .ShouldBeEquivalentTo(
                    new List<DockItemViewModel>() { tabNode.Tabs[0], tabNode.Tabs[1] },
                    $"Unmonitoring a {nameof(DockTabNodeViewModel)} should unhook all items.");
        }

        [Fact]
        public void Unmonitor_UnhooksNestedChildren()
        {
            TestVariables vars = new();

            DockSplitNodeViewModel splitNode = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            vars.Monitor.Monitor(splitNode);

            vars.Hooked
                .ShouldContain(
                    (splitNode.Children[0] as DockTabNodeViewModel)!.Tabs[0],
                    "The item in the split node child should have been hooked.");

            vars.Monitor.Unmonitor(splitNode);

            vars.Unhooked
                .ShouldContain(
                    (splitNode.Children[0] as DockTabNodeViewModel)!.Tabs[0],
                    "The item in the split node child should have been unhooked after unmonitoring the split node.");
        }

        private sealed class TestVariables
        {
            public TestVariables()
            {
                this.Monitor = new DockNodeMonitor(this.Hooked.Add, this.Unhooked.Add);
            }

            public List<DockItemViewModel> Hooked { get; } = [];

            public List<DockItemViewModel> Unhooked { get; } = [];

            public DockNodeMonitor Monitor { get; }
        }
    }
}
