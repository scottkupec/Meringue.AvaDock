// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockTabPanelTests
    {
        [AvaloniaFact]
        public void DockTabPanelStyle_DefinesDropTarget()
        {
            DockTabPanel panel = new();
            Border? dropTarget = null;

            panel.TemplateApplied += (_, e) =>
            {
                dropTarget = e.NameScope.Find<Border>("PART_DropTarget");
            };

            Window window = new() { Content = panel };
            window.Show();

            dropTarget
                .ShouldNotBeNull("PART_DropTarget must be findable for tests to pass.");
        }

        [AvaloniaFact]
        public void DragEnter_ShouldShowAdornersWhenValidDragData()
        {
            DockItemViewModel draggedItem = new() { Title = "Dragged" };
            DockTabNodeViewModel node = new();
            node.AddTab(new DockItemViewModel { Title = "Item 1" });

            DockTabPanel panel = new() { ItemsSource = node.ObservableTabs };
            Border container = new() { DataContext = node, Child = panel };
            Window window = new() { Content = container };
            window.Show();

            DataObject dragData = new();
            dragData.Set(DockContext.DragDropContextName, draggedItem);

            DragEventArgs args = new(
                DragDrop.DragEnterEvent,
                dragData,
                panel,
                new Point(10, 10),
                KeyModifiers.None);

            panel.RaiseEvent(args);

            args.Handled
                .ShouldBeTrue("DragEnter should be handled when valid drag data is present.");
        }

        [AvaloniaFact]
        public void DragOver_ShouldUpdateVisualFeedbackWhenPointerOverTabStrip()
        {
            DockItemViewModel draggedItem = new() { Title = "Dragged" };
            DockTabNodeViewModel node = new();
            node.AddTab(new DockItemViewModel { Title = "Item 1" });
            node.AddTab(new DockItemViewModel { Title = "Item 2" });

            DockTabPanel panel = new() { ItemsSource = node.ObservableTabs };
            Border container = new() { DataContext = node, Child = panel };
            Window window = new() { Content = container };
            window.Show();

            DataObject dragData = new();
            dragData.Set(DockContext.DragDropContextName, draggedItem);

            DragEventArgs args = new(
                DragDrop.DragOverEvent,
                dragData,
                panel,
                new Point(5, 5), // assume this hits tab header
                KeyModifiers.None);

            panel.RaiseEvent(args);

            args.Handled
                .ShouldBeTrue("DragOver should be handled when pointer is over tab strip.");

            args.DragEffects
                .ShouldBe(DragDropEffects.Move);
        }

        ////[AvaloniaFact]
        //// TODO: Refactor drag+drop to make this more testable.  As written, the current test
        ////       fires OnTabPointerPressed and OnTabPointerMoved, but not OnTabDropped. Once
        ////       fixed, fill in the other drag+drop tests as well.
        ////public void DragTab_ReordersTabsWhenDraggedOverAnother()
        ////{
        ////    DockItemViewModel tab1 = new() { Title = "Tab 1" };
        ////    DockItemViewModel tab2 = new() { Title = "Tab 2" };
        ////    DockTabNodeViewModel node = new();
        ////    node.AddTab(tab1);
        ////    node.AddTab(tab2);

        ////    DockTabPanel panel = new() { ItemsSource = node.ObservableTabs };
        ////    Border container = new() { DataContext = node, Child = panel };
        ////    Window window = new() { Content = container };
        ////    window.Show();
        ////    window.InvalidateVisual();
        ////    Thread.Sleep(1000);

        ////    Dispatcher.UIThread.RunJobs();
        ////    DumpVisualTree(window);
        ////    TabItem? tabItem1 = panel.ContainerFromIndex(0) as TabItem;
        ////    TabItem? tabItem2 = panel.ContainerFromIndex(1) as TabItem;

        ////    tabItem1!.AddHandler(DragDrop.DragEnterEvent, (_, __) => System.Diagnostics.Debug.WriteLine("DragEnter"));
        ////    tabItem1.AddHandler(DragDrop.DragOverEvent, (_, __) => System.Diagnostics.Debug.WriteLine("DragOver"));
        ////    tabItem1.AddHandler(DragDrop.DropEvent, (_, __) => System.Diagnostics.Debug.WriteLine("Drop"));

        ////    tabItem1
        ////        .ShouldNotBeNull("Tab 1 must be present.");

        ////    tabItem2
        ////        .ShouldNotBeNull("Tab 2 must be present.");

        ////    tabItem1!
        ////        .Presenter.ShouldNotBeNull("Tab 1 presenter must be available.");

        ////    tabItem2!
        ////        .Presenter.ShouldNotBeNull("Tab 2 presenter must be available.");

        ////    Matrix transform1 = tabItem1.Presenter.TransformToVisual(window)!.Value;
        ////    Matrix transform2 = tabItem2.Presenter.TransformToVisual(window)!.Value;

        ////    Point startPoint = transform2.Transform(tabItem2.Presenter.Bounds.Center);
        ////    Point endPoint = transform1.Transform(tabItem1.Presenter.Bounds.Center);
        ////    endPoint -= new Vector(5, 0);

        ////    IInputElement? hit = window.InputHitTest(endPoint);
        ////    System.Diagnostics.Debug.WriteLine($"Hit test at endPoint: {hit?.GetType().Name}");

        ////    System.Diagnostics.Debug.WriteLine($"Dragging from [{startPoint.X}, {startPoint.Y}] to [{endPoint.X}, {endPoint.Y}]");
        ////    Console.WriteLine($"Dragging from [{startPoint.X}, {startPoint.Y}] to [{endPoint.X}, {endPoint.Y}]");

        ////    window.MouseDown(startPoint, MouseButton.Left, RawInputModifiers.LeftMouseButton);
        ////    window.MouseMove(startPoint + GetDirection(startPoint, endPoint), RawInputModifiers.LeftMouseButton);
        ////    Point move = GetDirection(startPoint, endPoint);
        ////    System.Diagnostics.Debug.WriteLine($"Dragging from [{startPoint.X}, {startPoint.Y}] to [{move.X}, {move.Y}] to [{endPoint.X}, {endPoint.Y}]");
        ////    window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);
        ////    window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.LeftMouseButton);
        ////    System.Diagnostics.Debug.WriteLine("X");
        ////    ////window.MouseDown(startPoint, MouseButton.Left);
        ////    ////window.MouseMove(startPoint - new Point(1, 0), RawInputModifiers.LeftMouseButton); // https://github.com/AvaloniaUI/Avalonia/issues/17331
        ////    ////window.MouseMove(startPoint - new Point(10, 0), RawInputModifiers.LeftMouseButton); // https://github.com/AvaloniaUI/Avalonia/issues/17331
        ////    ////window.MouseMove(endPoint, RawInputModifiers.LeftMouseButton);                     // actual drag
        ////    ////window.MouseUp(endPoint, MouseButton.Left, RawInputModifiers.LeftMouseButton);
        ////    ////Dispatcher.UIThread.RunJobs();

        ////    node.ObservableTabs[0]
        ////        .ShouldBe(tab2, "Tab 2 should be reordered to first position.");

        ////    node.ObservableTabs[1]
        ////        .ShouldBe(tab1, "Tab 1 should be moved to second position.");
        ////}

        [AvaloniaFact]
        public void DropEvent_ShouldBeHandledWhenVisualTreeIsCorrect()
        {
            DockTabNodeViewModel viewModel = new();
            TestDockTabPanel panel = new()
            {
                ItemsSource = new ObservableCollection<Object>
                {
                    new DockItemViewModel { Title = "Item 1" },
                    new DockItemViewModel { Title = "Item 2" },
                },
            };

            Border container = new()
            {
                DataContext = viewModel,
                Child = panel,
            };

            Window window = new() { Content = container };
            window.Show();

            DataObject dragData = new();
            DockItemViewModel draggedItem = new();
            dragData.Set(DockContext.DragDropContextName, draggedItem);

            DragEventArgs args = new(
                DragDrop.DropEvent,
                dragData,
                panel,
                new Point(10, 10),
                KeyModifiers.None);

            panel.RaiseEvent(args);

            panel.DropEventHandlerCalled
                .ShouldBeTrue("DropEvent handler should be called when drag-drop is routed correctly.");
        }

        [AvaloniaFact]
        public void ItemsSource_ChangeTriggersVisibilityUpdate()
        {
            DockTabPanel panel = new();
            ObservableCollection<Object> items = [];
            panel.ItemsSource = items;

            items.Add(new DockItemViewModel());
            items.Add(new DockItemViewModel());

            panel.ShouldShowTabStrip
                .ShouldBeTrue($"{nameof(panel.ShouldShowTabStrip)} should be true after adding two items.");
        }

        [AvaloniaFact]
        public void SelectedItem_BindingUpdatesViewModelSelected()
        {
            DockTabNodeViewModel tabNode = new();
            DockItemViewModel item1 = new() { Title = "Item 1" };
            DockItemViewModel item2 = new() { Title = "Item 2" };
            tabNode.AddTab(item1);
            tabNode.AddTab(item2);

            ContentControl host = new()
            {
                Content = tabNode,
            };

            Window window = new() { Content = host };
            window.Show();

            DockTabPanel? panel = host.GetVisualDescendants()
                .OfType<DockTabPanel>()
                .FirstOrDefault();

            panel
                .ShouldNotBeNull($"Sanity: The {nameof(DockTabPanel)} must be found to run this test.");

            panel!.SelectedItem = item2;

            tabNode.Selected
                .ShouldBe(item2, "Selected tab in view model should reflect control selection via binding.");
        }

        [AvaloniaFact]
        public void ShouldShowTabStrip_IsFalseWhenNoItemsPresent()
        {
            DockTabPanel panel = new();
            ObservableCollection<Object> items = [];

            panel.ItemsSource = items;

            panel.ShouldShowTabStrip
                .ShouldBeFalse($"{nameof(panel.ShouldShowTabStrip)} should be false when no items are present.");
        }

        [AvaloniaFact]
        public void ShouldShowTabStrip_IsFalseWhenOneItemPresent()
        {
            DockTabPanel panel = new();
            ObservableCollection<Object> items =
            [
                new DockItemViewModel()
            ];

            panel.ItemsSource = items;

            panel.ShouldShowTabStrip
                .ShouldBeFalse($"{nameof(panel.ShouldShowTabStrip)} should be false when only one item is present.");
        }

        [AvaloniaFact]
        public void ShouldShowTabStrip_IsTrueWhenTwoItemsPresent()
        {
            DockTabPanel panel = new();
            ObservableCollection<Object> items =
            [
                new DockItemViewModel(),
                new DockItemViewModel()
            ];

            panel.ItemsSource = items;

            panel.ShouldShowTabStrip
                .ShouldBeTrue($"{nameof(panel.ShouldShowTabStrip)} should be true when two items are present.");
        }

        /////// <summary>Support method for drag+drop tests.</summary>
        /////// <remarks>See https://github.com/AvaloniaUI/Avalonia/issues/17331</remarks>
        ////private static Point GetDirection(Point start, Point end)
        ////{
        ////    Int32 x = Math.Sign(end.X - start.X);
        ////    Int32 y = Math.Sign(end.Y - start.Y);
        ////
        ////    return new Point(x, y);
        ////}

        // Test stub so we can verify handlers are called and tests that validate
        // "do nothing" conditions are passing with false positives.
        private sealed class TestDockTabPanel : DockTabPanel
        {
            public Boolean DropEventHandlerCalled { get; private set; } = false;

            protected override void OnInitialized()
            {
                base.OnInitialized();

                this.AddHandler(DragDrop.DropEvent, (sender, eventArgs) =>
                {
                    Control? visual = (eventArgs.Source as Visual)?
                        .GetVisualAncestors()
                        .OfType<Control>()
                        .FirstOrDefault(c => c.DataContext is DockTabNodeViewModel);

                    visual.ShouldNotBeNull("Sanity: Visual tree not set up correctly.");
                    this.DropEventHandlerCalled = true;
                });
            }
        }
    }
}
