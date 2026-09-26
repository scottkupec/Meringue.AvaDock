// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia.Layout;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.ViewModels.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockNodeViewModelTests
    {
        [Fact]
        public void FindAncestorSplitNode_ReturnsCorrectNode()
        {
            String itemId = "item";
            DockItemViewModel item = new()
            {
                Id = itemId,
            };

            DockTabNodeViewModel tab = new()
            {
                Id = "tab",
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = "split",
            };

            tab.AddTab(item);
            split.AddChild(tab);

            split.FindAncestorSplitNode(tab)
                .ShouldBe(split, $"{nameof(DockNodeViewModel.FindAncestorSplitNode)} should return the split node that contains the item.");
        }

        [Fact]
        public void FindAncestorTabNode_ReturnsCorrectNode()
        {
            String itemId = "item";
            DockItemViewModel item = new()
            {
                Id = itemId,
            };

            DockTabNodeViewModel tab = new()
            {
                Id = "tab",
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = "split",
            };

            tab.AddTab(item);
            split.AddChild(tab);

            split.FindAncestorTabNode(itemId)
                .ShouldBe(tab, $"{nameof(DockNodeViewModel.FindAncestorTabNode)} should return the tab node that contains the item.");
        }

        [Fact]
        public void FindDescendentItem_ReturnsCorrectItem()
        {
            DockItemViewModel item = new()
            {
                Id = "item",
            };

            DockTabNodeViewModel tab = new()
            {
                Id = "tab",
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = "split",
            };

            tab.AddTab(item);
            split.AddChild(tab);

            split.FindDescendentItem<DockItemViewModel>(item.Id)
                .ShouldBe(item, $"{nameof(DockNodeViewModel.FindDescendentItem)} should find the correct item starting at split node.");

            tab.FindDescendentItem<DockItemViewModel>(item.Id)
                .ShouldBe(item, $"{nameof(DockNodeViewModel.FindDescendentItem)} should find the correct item starting at a tab node.");
        }

        [Fact]
        public void FindDescendentNode_ReturnsCorrectItem()
        {
            String itemId = "item";
            DockItemViewModel item = new()
            {
                Id = itemId,
            };

            DockTabNodeViewModel tab = new()
            {
                Id = "tab",
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = "split",
            };

            tab.AddTab(item);
            split.AddChild(tab);

            split.FindDescendentNode(tab.Id)
                .ShouldBe(tab, $"{nameof(DockNodeViewModel.FindDescendentNode)} should find the correct tab starting at split node.");

            tab.FindDescendentNode(tab.Id)
                .ShouldBe(tab, $"{nameof(DockNodeViewModel.FindDescendentNode)} should find self.");

            tab.FindDescendentNode(split.Id)
                .ShouldBeNull($"{nameof(DockNodeViewModel.FindDescendentNode)} should not find parent node.");
        }

        [Fact]
        public void FindFirstTabNode_ReturnsCorrectTab()
        {
            DockTabNodeViewModel tab1 = new()
            {
                Id = "tab1",
            };

            DockTabNodeViewModel tab2 = new()
            {
                Id = "tab2",
            };

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = "split",
            };

            split.AddChild(tab1);
            split.AddChild(tab2);

            split.FindFirstTabNode()
                .ShouldBe(tab1, $"{nameof(DockNodeViewModel.FindFirstTabNode)} should find the first tab node.");
        }
    }
}
