// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.Threading;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.UnitTests;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockControlTests
    {
        [AvaloniaFact]
        public void FloatItem_RemovesFromMainAndAppearsInFloatingWindow()
        {
            Window window = new();

            DockItemViewModel item = new()
            {
                Id = "item1",
                Title = "Item 1",
                Context = new Object(),
            };

            DockControlManager controlManager = MakeRoot();
            DockControl control = new()
            {
                DataContext = controlManager,
            };

            controlManager.PrimaryWorkspace.DockTree.FindFirstTabNode()?.AddTab(item);

            control.ApplyTemplate();

            Dispatcher.UIThread.RunJobs();

            window.Content = controlManager.PrimaryWorkspace;
            window.Show();

            controlManager.PrimaryWorkspace.DockTree.FindItem<DockItemViewModel>(item.Id)
                .ShouldNotBeNull($"Sanity: '{nameof(item)}' should initially be in {nameof(DockControlManager.PrimaryWorkspace)}.");

            controlManager.FloatItem(item, null, null);
            Dispatcher.UIThread.RunJobs();

            // Assert
            controlManager.PrimaryWorkspace.DockTree.FindItem<DockItemViewModel>(item.Id)
                .ShouldBeNull($"'{nameof(item)}' should be removed from {nameof(DockControlManager.PrimaryWorkspace)}.");

            controlManager.SecondaryWorkspaces.Count()
                .ShouldBe(1, $"A {nameof(controlManager.SecondaryWorkspaces)} should have been created.");

            controlManager.SecondaryWorkspaces
                .Any(workspace => workspace.DockTree.FindItem<DockItemViewModel>(item.Id) == item)
                .ShouldBeTrue($"'{nameof(item)}' should be added to one of the {nameof(DockControlManager.SecondaryWorkspaces)}.");
        }

        [AvaloniaFact]
        public void ManagerProperty_UpdatesWorkspaceBinding()
        {
            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Horizontal));
            DockControlManager manager = new(workspace);
            DockControl control = new()
            {
                Manager = manager,
            };

            DockWorkspaceManager? boundWorkspace = control.Workspace;

            // Assert
            boundWorkspace
                .ShouldBe(workspace, $"{nameof(DockControl.Workspace)} should reflect the {nameof(DockControlManager.PrimaryWorkspace)} of the assigned {nameof(DockControlManager)}.");
        }

        private static DockControlManager MakeRoot()
        {
            DockSplitNodeViewModel split = new(Orientation.Horizontal);
            DockWorkspaceManager workspace = new(split);
            split.AddChild(new DockTabNodeViewModel());
            DockControlManager control = new(workspace);
            control.WindowManager = new WindowManager(new TestWindowFactory());
            return control;
        }
    }
}
