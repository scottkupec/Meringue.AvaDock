// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.VisualTree;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockItemTests
    {
        [AvaloniaFact]
        public void PointerPressed_InitiatesDragDrop()
        {
            // Arrange
            DockItem dockItem = new()
            {
                DataContext = new DockItemViewModel(),
            };

            Window window = new() { Content = dockItem };
            window.Show();

            Border? titleBar = dockItem.GetVisualDescendants()
                .OfType<Border>()
                .FirstOrDefault(b => b.Name == "PART_TitleBar");

            titleBar
                .ShouldNotBeNull("Sanity: The title bar must be found for the test to be valid.");

            Boolean pressed = false;
            titleBar!.AddHandler(InputElement.PointerPressedEvent, (sender, eventArgs) => pressed = true);

            Point center = titleBar.GetTransformedBounds()!.Value.Bounds.Center;

            // Act: simulate real routed pointer click
            window.MouseDown(center, MouseButton.Left);
            window.MouseUp(center, MouseButton.Left); // Failing to MouseUp() hangs the test entirely

            // Assert
            pressed
                .ShouldBeTrue("PointerPressed event should be raised on PART_TitleBar");
        }

        [AvaloniaFact]
        public void TitleProperty_ShouldBindCorrectly()
        {
            // Arrange and act
            DockItem dockItem = new()
            {
                Height = 100,
                Title = "Test Title",
                Width = 200,
            };

            // Assert
            dockItem.Title
                .ShouldBe("Test Title", $"{nameof(DockItem.Title)} should reflect the assigned value.");
        }
    }
}
