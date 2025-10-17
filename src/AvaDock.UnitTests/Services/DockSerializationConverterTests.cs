// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Headless.XUnit;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.UnitTests;
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
            // Arrange
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

            workspace.Items
                .ShouldContain(visibleItem, "Sanity: Visible item should be in workspace before serialization.");

            workspace.MinimizedItems
                .ShouldContain(minimizedItem, "Sanity: Minimized item should be in workspace before serialization.");

            control.HiddenItems
                .ShouldContain(hiddenItem, "Sanity: Hidden item should be in control before serialization.");

            DockControlData serialized = DockSerializationConverter.BuildLayout<DockItemViewModel>(control);
            DockControlManager deserializedControl = DockSerializationConverter.BuildViewModel<DockItemViewModel>(serialized);

            DockItemViewModel? restoredVisible = deserializedControl.FindItem(visibleItem.Id);
            restoredVisible
                .ShouldNotBeNull("Visible item should be restored in workspace.");

            DockItemViewModel? restoredMinimized = deserializedControl.FindItem(minimizedItem.Id);
            restoredMinimized
                .ShouldNotBeNull("Minimized item should be restored in workspace.");

            deserializedControl.FindOwningTabNode(minimizedItem.Id)
                .ShouldBeNull("A minimized item should not be restored to a tab.");

            DockItemViewModel? restoredHidden = deserializedControl.FindItem(hiddenItem.Id);
            restoredHidden.
                ShouldNotBeNull("Hidden item should be restored in control.");

            deserializedControl.FindOwningTabNode(hiddenItem.Id)
                .ShouldBeNull("A hidden item should not be restored to a tab.");
        }
    }
}
