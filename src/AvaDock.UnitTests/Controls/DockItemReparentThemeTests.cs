// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    /// <summary>Regression tests verifying that dock item content remains templated after reparenting.</summary>
    /// <remarks>
    /// In Avalonia 12 a control's logical parent is only ever assigned while the control
    /// has no logical parent. When a <see cref="DockControl"/> is moved or an item is floated, the discarded
    /// <see cref="DockSplitPanel"/> template instances still hold binding connections to the shared view models,
    /// and their ContentPresenters retain the logical parenting on hosted item content. That prevents lives hosts
    /// from reparenting the content. When the stale tree is detached the content is left with a broken parent chain
    /// which makes implicit theme lookup fail.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockItemReparentThemeTests
    {
        [AvaloniaFact]
        public void FloatItem_ItemContentRemainsTemplated()
        {
            TestVariables testVariables = new();

            DockTabNodeViewModel tabNode = new();
            testVariables.RootNode.AddChild(tabNode);

            TextBox textBox = new() { Text = "hello", };
            DockItemViewModel item = new() { Id = "t1", Title = "Tab 1", Context = textBox, };

            tabNode.AddTab(item);

            testVariables.MainWindow.Content = new DockControl
            {
                Manager = testVariables.Manager,
                DataContext = testVariables.Manager,
            };

            DockItemReparentThemeTests.LayoutWindow(testVariables.MainWindow);

            testVariables.Manager.FloatItem(item, null, null);
            Dispatcher.UIThread.RunJobs();

            WorkspaceWindow? floatWindow = testVariables.WindowFactory.LastCreated;

            floatWindow.ShouldNotBeNull(
                $"{nameof(floatWindow)} should have been created.");

            DockItemReparentThemeTests.LayoutWindow(floatWindow);

            floatWindow.GetSelfAndVisualDescendants().OfType<TextBox>().Count().ShouldBe(
                1,
                $"{nameof(floatWindow)} should host the  {nameof(textBox)} content exactly once.");

            testVariables.MainWindow.GetSelfAndVisualDescendants().ShouldNotContain(
                textBox,
                $"{nameof(testVariables.MainWindow)} should no longer host the {nameof(textBox)} content.");

            textBox.Template.ShouldNotBeNull(
                $"{nameof(textBox)} should remain templated after floating.");

            ((ILogical)textBox).IsAttachedToLogicalTree.ShouldBeTrue(
                $"{nameof(textBox)} should remain in the logical tree after floating.");
        }

        [AvaloniaFact]
        public void RequestMove_ItemContentRemainsTemplated()
        {
            TestVariables testVariables = new();

            DockTabNodeViewModel sourceTab = new();
            DockTabNodeViewModel targetTab = new();

            testVariables.RootNode.AddChild(sourceTab);
            testVariables.RootNode.AddChild(targetTab);

            TextBox textBox = new() { Text = "hello", };
            DockItemViewModel item = new() { Id = "t1", Title = "Tab 1", Context = textBox, };

            sourceTab.AddTab(item);

            testVariables.MainWindow.Content = new DockControl
            {
                Manager = testVariables.Manager,
                DataContext = testVariables.Manager,
            };

            DockItemReparentThemeTests.LayoutWindow(testVariables.MainWindow);

            item.RequestMove(sourceTab, targetTab, MovePlacement.On, null);
            Dispatcher.UIThread.RunJobs();
            DockItemReparentThemeTests.LayoutWindow(testVariables.MainWindow);

            testVariables.MainWindow.GetSelfAndVisualDescendants().OfType<TextBox>().Count().ShouldBe(
                1,
                $"{nameof(textBox)} should be hosted exactly once after the move.");

            textBox.Template.ShouldNotBeNull(
                $"{nameof(textBox)} should remain templated after the move.");

            (textBox as ILogical).IsAttachedToLogicalTree.ShouldBeTrue(
                $"{nameof(textBox)} should remain in the logical tree after the move.");
        }

        private static void LayoutWindow(Window window)
        {
            Dispatcher.UIThread.RunJobs();
            Size size = new(800, 600);
            window.Measure(size);
            window.Arrange(new Rect(size));
            Dispatcher.UIThread.RunJobs();
        }

        private sealed class TestVariables
        {
            public TestVariables()
            {
                this.MainWindow = new() { Width = 800, Height = 600 };
                this.WindowFactory = new(this.MainWindow);
                this.RootNode = new(Orientation.Horizontal) { Id = "root", };
                this.Workspace = new(this.RootNode);
                this.Manager = new(this.Workspace);
                this.Manager.WindowManager = new WindowManager(this.WindowFactory);
                this.MainWindow.Show();
            }

            public Window MainWindow { get; init; }

            public DockControlManager Manager { get; }

            public DockSplitNodeViewModel RootNode { get; }

            public WorkspaceWindowFactory WindowFactory { get; }

            public DockWorkspaceManager Workspace { get; }
        }

        private sealed class WorkspaceWindowFactory : IWindowFactory
        {
            private readonly Window owner;

            public WorkspaceWindowFactory(Window owner)
            {
                this.owner = owner;
            }

            public WorkspaceWindow? LastCreated { get; private set; }

            public IWindow MainWindow => new AvaloniaWindowAdapter(this.owner);

            public IWindow CreateWindow()
            {
                this.LastCreated = new WorkspaceWindow(this.owner);
                return new AvaloniaWindowAdapter(this.LastCreated);
            }
        }
    }
}
