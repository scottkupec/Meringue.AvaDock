// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Meringue.AvaDock.Managers;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    // TODO: Render() tests once updated to Avalonia 12 and DrawingContext is more testable.
    public sealed class TabPanelDropAdornerTests
    {
        [AvaloniaFact]
        public void Constructor_SetsDefaults()
        {
            TabPanelDropAdorner adorner = new();

            adorner.Focusable
                .ShouldBeFalse($"Default value for {nameof(TabPanelDropAdorner.Focusable)} should be correct.");

            adorner.HoveredZone
                .ShouldBe(DropZone.None, $"Default value for {nameof(TabPanelDropAdorner.HoveredZone)} should be correct.");

            adorner.IsHitTestVisible
                .ShouldBeFalse($"Default value for {nameof(TabPanelDropAdorner.IsHitTestVisible)} should be correct.");
        }

        [AvaloniaTheory]
        // Top left corner validation
        [InlineData(50, 50, DropZone.Center)]
        [InlineData(49, 50, DropZone.Left)]
        [InlineData(50, 49, DropZone.Top)]
        // Bottom right corner validation
        [InlineData(150, 150, DropZone.Center)]
        [InlineData(151, 150, DropZone.Right)]
        [InlineData(150, 151, DropZone.Bottom)]
        public void UpdateTarget_IsCorrectForBCornerBoundaries(Double x, Double y, DropZone expected)
        {
            Grid root = new()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            TabPanelDropAdorner adorner = new();
            Border element = new()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            root.Children.Add(element);
            root.Children.Add(adorner);

            Window window = new()
            {
                Content = root,
                Height = 200,
                Width = 200,
            };

            window.Show();

            adorner.UpdateTarget(element, new Point(x, y));

            adorner.HoveredZone
                .ShouldBe(expected, $"Expected zone '{expected}' for point ({x},{y})");
        }

        [AvaloniaFact]
        public void UpdateTarget_SetsAdornedElement()
        {
            TabPanelDropAdorner adorner = new();
            Border element = new() { Width = 100, Height = 100 };

            element.Measure(Size.Infinity);
            element.Arrange(new Rect(0, 0, 100, 100));

            adorner.UpdateTarget(element, new Point(10, 10));

            adorner.AdornedElement
                .ShouldBe(element, $"{nameof(TabPanelDropAdorner.AdornedElement)} should be set correctly.");
        }

        [AvaloniaFact]
        public void UpdateTarget_ThrowsOnNullAdornedElement()
        {
            TabPanelDropAdorner adorner = new();

            Should.
                Throw<ArgumentNullException>(() => adorner.UpdateTarget(null!, new Point(0, 0)))
                .ParamName
                .ShouldBe("adornedElement", $"A null {nameof(Control)} should not be valid.");
        }
    }
}
