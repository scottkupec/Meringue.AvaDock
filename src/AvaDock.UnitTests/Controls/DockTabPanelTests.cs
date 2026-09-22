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

            panel.SelectedItem = item2;

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
