// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.UnitTests;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.ViewModels.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockTabNodeViewModelTests
    {
        [Fact]
        public void ClearTabs_RemovesAllTabs()
        {
            DockTabNodeViewModel tabNode = DockTree.Tab("item-1", "item-2");

            tabNode.Tabs.Count
                .ShouldBe(2, $"Sanity: TabNode should contain two tabs before {nameof(DockTabNodeViewModel.ClearTabs)} is called.");

            tabNode.ClearTabs();

            tabNode.Tabs.Count
                .ShouldBe(0, $"TabNode should contain no tabs after {nameof(DockTabNodeViewModel.ClearTabs)} is called.");
        }

        [Fact]
        public void Constructor_InitializesEmptyTabs()
        {
            DockTabNodeViewModel viewModel = new();

            viewModel.Tabs
                .ShouldNotBeNull($"{nameof(viewModel.Tabs)} should not be null.");

            viewModel.Tabs
                .ShouldBeEmpty($"{nameof(viewModel.Tabs)} should be empty by default.");
        }

        [Fact]
        public void RemovingTab_StopsShowingTabStrip()
        {
            DockTabNodeViewModel tabNode = DockTree.Tab("item-1", "item-2");

            tabNode.RemoveTab(tabNode.Tabs[0]);

            tabNode.Tabs.Count
                .ShouldBe(1, $"Removing a {nameof(DockItemViewModel)} should reduce the number of {nameof(DockTabNodeViewModel.Tabs)} being present.");
        }
    }
}
