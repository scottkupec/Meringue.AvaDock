// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockTreeTests
    {
        [Fact]
        public void Workspace_HiddenItems_AreRoutedToHiddenItems()
        {
            DockItemViewModel hiddenItem = DockTree.Item(null, "hidden-1", "Hidden One");

            DockWorkspaceManager workspace = DockTree.Workspace(
                dockTree: null,
                minimized: null,
                hidden: DockTree.Hidden(hiddenItem));

            workspace.HiddenItems.ShouldContain(
                hiddenItem,
                "A hidden item passed to DockTree.Workspace should be added to the workspace and routed to HiddenItems.");

            workspace.MinimizedItems.ShouldNotContain(
                hiddenItem,
                "A hidden item must not be routed to MinimizedItems.");
        }

        [Fact]
        public void Workspace_HiddenAndMinimizedItems_EachRouteToTheirOwnCollection()
        {
            DockItemViewModel minimizedItem = DockTree.Item(null, "minimized-2", "Minimized Two");
            DockItemViewModel hiddenItem = DockTree.Item(null, "hidden-2", "Hidden Two");

            DockWorkspaceManager workspace = DockTree.Workspace(
                dockTree: null,
                minimized: DockTree.Minimized(minimizedItem),
                hidden: DockTree.Hidden(hiddenItem));

            workspace.MinimizedItems.ShouldContain(
                minimizedItem,
                "A minimized item should be routed to MinimizedItems.");

            workspace.HiddenItems.ShouldContain(
                hiddenItem,
                "A hidden item should be routed to HiddenItems.");

            workspace.MinimizedItems.ShouldNotContain(
                hiddenItem,
                "A hidden item must not appear in MinimizedItems.");

            workspace.HiddenItems.ShouldNotContain(
                minimizedItem,
                "A minimized item must not appear in HiddenItems.");
        }

        [Fact]
        public void Workspace_MinimizedItems_AreRoutedToMinimizedItems()
        {
            DockItemViewModel minimizedItem = DockTree.Item(null, "minimized-1", "Minimized One");

            DockWorkspaceManager workspace = DockTree.Workspace(
                dockTree: null,
                minimized: DockTree.Minimized(minimizedItem),
                hidden: null);

            workspace.MinimizedItems.ShouldContain(
                minimizedItem,
                "A minimized item passed to DockTree.Workspace should be added to the workspace and routed to MinimizedItems.");

            workspace.HiddenItems.ShouldNotContain(
                minimizedItem,
                "A minimized item must not be routed to HiddenItems.");
        }
    }
}
