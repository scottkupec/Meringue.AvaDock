// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Headless.XUnit;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Services.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockSerializationConverterTests
    {
        [AvaloniaFact]
        public void Layout_ShouldRoundTrip()
        {
            DockItemViewModel visibleItem = new() { Title = "Visible Tab" };
            DockItemViewModel minimizedItem = new() { Title = "Minimized Tab" };
            DockItemViewModel hiddenItem = new() { Title = "Hidden Tab" };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(visibleItem);
            tabNode.AddTab(minimizedItem);
            tabNode.AddTab(hiddenItem);

            DockWorkspaceManager workspace = new(DockTree.Horizontal(tabNode));
            DockControlManager control = new(workspace);

            minimizedItem.MinimizeCommand.Execute(null);
            hiddenItem.HideCommand.Execute(null);

            workspace.Items.ShouldContain(visibleItem, "Sanity: Visible item should be in workspace before serialization.");
            workspace.MinimizedItems.ShouldContain(minimizedItem, "Sanity: Minimized item should be in workspace before serialization.");
            control.HiddenItems.ShouldContain(hiddenItem, "Sanity: Hidden item should be in control before serialization.");

            DockControlData serialized = DockSerializationConverter.BuildLayout<DockItemViewModel>(control);
            DockControlManager deserializedControl = DockSerializationConverter.BuildViewModel<DockItemViewModel>(serialized);

            // Assert – serialization structure
            serialized.PrimaryWorkspace.ShouldNotBeNull("Primary workspace should be serialized.");
            serialized.PrimaryWorkspace.Minimized.ShouldNotBeNull("Minimized items should be serialized.");
            serialized.PrimaryWorkspace.Minimized.Count.ShouldBe(1, "Exactly one minimized item should be serialized.");
            serialized.PrimaryWorkspace.Hidden.ShouldNotBeNull("Hidden items should be serialized.");
            serialized.PrimaryWorkspace.Hidden.Count.ShouldBe(1, "Exactly one hidden item should be serialized.");

            // Assert – visible item restored
            DockItemViewModel? restoredVisible = deserializedControl.FindItem(visibleItem.Id);
            restoredVisible.ShouldNotBeNull("Visible item should be restored in workspace.");
            restoredVisible.Title.ShouldBe(visibleItem.Title, "Visible item title should be preserved.");
            deserializedControl.PrimaryWorkspace.Items.ShouldContain(restoredVisible, "Visible item should be in primary workspace items.");
            deserializedControl.PrimaryWorkspace.DockTree.FindAncestorTabNode(visibleItem.Id).ShouldNotBeNull("Visible item should be owned by a tab node.");

            // Assert – minimized item restored
            DockItemViewModel? restoredMinimized = deserializedControl.FindItem(minimizedItem.Id);
            restoredMinimized.ShouldNotBeNull("Minimized item should be restored in workspace.");
            restoredMinimized.Title.ShouldBe(minimizedItem.Title, "Minimized item title should be preserved.");
            deserializedControl.PrimaryWorkspace.MinimizedItems.ShouldContain(restoredMinimized, "Minimized item should be in MinimizedItems collection.");
            deserializedControl.PrimaryWorkspace.DockTree.FindAncestorTabNode(minimizedItem.Id).ShouldBeNull("A minimized item should not be restored to a tab.");

            // Assert – hidden item restored
            DockItemViewModel? restoredHidden = deserializedControl.FindItem(hiddenItem.Id);
            restoredHidden.ShouldNotBeNull("Hidden item should be restored in control.");
            restoredHidden.Title.ShouldBe(hiddenItem.Title, "Hidden item title should be preserved.");
            deserializedControl.HiddenItems.ShouldContain(restoredHidden, "Hidden item should be in HiddenItems collection.");
            deserializedControl.PrimaryWorkspace.DockTree.FindAncestorTabNode(hiddenItem.Id).ShouldBeNull("A hidden item should not be restored to a tab.");
        }

        [AvaloniaFact]
        public void Layout_ShouldRoundTripHiddenMinimizedState()
        {
            DockItemViewModel item = new() { Title = "Hidden Minimized Tab" };

            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockWorkspaceManager workspace = new(DockTree.Horizontal(tabNode));
            DockControlManager control = new(workspace);

            item.MinimizeCommand.Execute(null);
            item.HideCommand.Execute(null);

            workspace.MinimizedItems.ShouldNotContain(item, "Sanity: Minimized item should not be in workspace before serialization.");
            control.HiddenItems.ShouldContain(item, "Sanity: Hidden item should be in control before serialization.");

            DockControlData serialized = DockSerializationConverter.BuildLayout<DockItemViewModel>(control);
            serialized.PrimaryWorkspace.ShouldNotBeNull("Primary workspace should be serialized.");
            serialized.PrimaryWorkspace.Hidden.ShouldNotBeNull("Hidden items should be serialized.");
            serialized.PrimaryWorkspace.Hidden.ShouldHaveSingleItem("Exactly one hidden item should be serialized.");
            serialized.PrimaryWorkspace.Hidden[0].State.ShouldBe(DockItemState.Minimized | DockItemState.Hidden, "State should preserve both the Minimized and Hidden flags when serialized.");

            DockControlManager deserializedControl = DockSerializationConverter.BuildViewModel<DockItemViewModel>(serialized);

            DockItemViewModel? restoredItem = deserializedControl.FindItem(item.Id);
            restoredItem.ShouldNotBeNull("Hidden minimized item should be restored in control.");
            DockContext.GetItemState(restoredItem).ShouldBe(DockItemState.Minimized | DockItemState.Hidden, "State should restore both the Minimized and Hidden flags.");
            deserializedControl.HiddenItems.ShouldContain(restoredItem, "Hidden minimized item should be in HiddenItems collection.");
            deserializedControl.PrimaryWorkspace.MinimizedItems.ShouldNotContain(restoredItem, "Hidden minimized item should not be visible in the minimized bar.");
            deserializedControl.PrimaryWorkspace.DockTree.FindAncestorTabNode(item.Id).ShouldBeNull("A hidden item should not be restored to a tab.");
        }
    }
}
