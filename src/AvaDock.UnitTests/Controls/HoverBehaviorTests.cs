// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Tests.Controls
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class HoverBehaviorTests
    {
        [AvaloniaFact]
        public void EnableHoverTracking_TogglesEventHandlers()
        {
            Button control = new();

            HoverBehavior.SetEnableHoverTracking(control, true);
            HoverBehavior.GetEnableHoverTracking(control)
                .ShouldBeTrue("EnableHoverTracking should be true after setting.");

            HoverBehavior.SetEnableHoverTracking(control, false);
            HoverBehavior.GetEnableHoverTracking(control)
                .ShouldBeFalse("EnableHoverTracking should be false after disabling.");
        }

        [AvaloniaFact]
        public async Task HoverTracking_ClearsHoveredItem_OnPointerExit()
        {
            DockItemViewModel item = new();
            DockWorkspaceManager workspace = HoverBehaviorTests.CreateWorkspaceWithMinimizedItem(item);

            Button control = new()
            {
                DataContext = item,
            };

            HoverBehavior.SetEnableHoverTracking(control, true);

            Panel host = new()
            {
                Width = 100,
                Height = 100,
                Children = { control },
            };

            host.Measure(Size.Infinity);
            host.Arrange(new Rect(0, 0, 100, 100));

            PointerEventArgs enterArgs = HoverBehaviorTests.CreatePointerEvent(control, host, InputElement.PointerEnteredEvent);
            await Dispatcher.UIThread.InvokeAsync(() => control.RaiseEvent(enterArgs));

            await Task.Delay(5).ConfigureAwait(false);

            PointerEventArgs exitArgs = HoverBehaviorTests.CreatePointerEvent(control, host, InputElement.PointerExitedEvent);
            await Dispatcher.UIThread.InvokeAsync(() => control.RaiseEvent(exitArgs));

            await Task.Delay(400).ConfigureAwait(false);

            workspace.HoveredItem
                .ShouldBeNull("HoveredItem should be cleared after pointer exit.");
        }

        [AvaloniaFact]
        public async Task HoverTracking_DoesNotSetHoveredItem_IfNotMinimized()
        {
            DockItemViewModel item = new();
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel root = new(Orientation.Horizontal);
            root.AddChild(tabNode);

            DockWorkspaceManager workspace = new(root);
            DockContext.SetWorkspace(item, workspace);

            Button control = new()
            {
                DataContext = item,
            };

            HoverBehavior.SetEnableHoverTracking(control, true);

            Panel host = new()
            {
                Width = 100,
                Height = 100,
                Children = { control },
            };

            host.Measure(Size.Infinity);
            host.Arrange(new Rect(0, 0, 100, 100));

            PointerEventArgs enterArgs = HoverBehaviorTests.CreatePointerEvent(control, host, InputElement.PointerEnteredEvent);
            await Dispatcher.UIThread.InvokeAsync(() => control.RaiseEvent(enterArgs));

            await Task.Delay(500).ConfigureAwait(false);

            workspace.HoveredItem
                .ShouldBeNull("HoveredItem should not be set if item is not minimized.");
        }

        [AvaloniaFact]
        public async Task HoverTracking_SetsHoveredItem_AfterDelay()
        {
            DockItemViewModel item = new();
            DockWorkspaceManager workspace = HoverBehaviorTests.CreateWorkspaceWithMinimizedItem(item);

            Button control = new()
            {
                DataContext = item,
            };

            HoverBehavior.SetEnableHoverTracking(control, true);

            Panel host = new()
            {
                Width = 100,
                Height = 100,
                Children = { control },
            };

            host.Measure(Size.Infinity);
            host.Arrange(new Rect(0, 0, 100, 100));

            PointerEventArgs enterArgs = HoverBehaviorTests.CreatePointerEvent(control, host, InputElement.PointerEnteredEvent);
            await Dispatcher.UIThread.InvokeAsync(() => control.RaiseEvent(enterArgs));

            await Task.Delay(500).ConfigureAwait(false);

            workspace.HoveredItem
                .ShouldBe(item, "HoveredItem should be set after hover delay.");
        }

        private static PointerEventArgs CreatePointerEvent(Control target, Visual root, RoutedEvent routedEvent)
        {
            PointerPointProperties properties = new();
            using Pointer pointer = new(0, PointerType.Mouse, true);
            Point position = new(10, 10);
            UInt64 timestamp = 0;

            return new PointerEventArgs(
                routedEvent,
                target,
                pointer,
                root,
                position,
                timestamp,
                properties,
                KeyModifiers.None);
        }

        private static DockWorkspaceManager CreateWorkspaceWithMinimizedItem(DockItemViewModel item)
        {
            DockTabNodeViewModel tabNode = new();
            tabNode.AddTab(item);

            DockSplitNodeViewModel root = new(Orientation.Horizontal);
            root.AddChild(tabNode);

            DockWorkspaceManager workspace = new(root);
            DockContext.SetWorkspace(item, workspace);
            item.MinimizeCommand.Execute(null);
            workspace.MinimizedItems.Count.ShouldBe(1, "Item should be in MinimizedTabs.");

            return workspace;
        }
    }
}
