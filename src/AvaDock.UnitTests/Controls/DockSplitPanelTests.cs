// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Meringue.AvaDock.ViewModels;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockSplitPanelTests
    {
        [AvaloniaFact]
        public void GridSplitters_AreInsertedBetweenChildren()
        {
            Int32 childCountToAdd = 3;

            DockSplitNodeViewModel viewModel = new(Orientation.Vertical);

            for (Int32 index = 0; index < childCountToAdd; index++)
            {
                viewModel.AddChild(new DockTabNodeViewModel());
            }

            DockSplitPanel panel = new()
            {
                DataContext = viewModel,
            };

            Window window = new() { Content = panel };
            window.Show();

            Grid? container = panel.GetVisualDescendants().OfType<Grid>().FirstOrDefault();

            container.ShouldNotBeNull("Expected Grid container to be present in visual tree");

            Int32 splitterCount = container!.Children.OfType<GridSplitter>().Count();
            Int32 expectedSplitterCount = childCountToAdd - 1;

            splitterCount.ShouldBe(
                expectedSplitterCount,
                $"Expected {expectedSplitterCount} GridSplitters between {childCountToAdd} children, but found {splitterCount}");
        }

        [AvaloniaFact]
        public void SaveCurrentSizes_IsTriggeredByRebuildLayout()
        {
            List<(DockTabNodeViewModel Content, Double Size)> childrenToAdd =
                [
                    // There are sets of sizes that will fail even though things are working.
                    // The provided sizes generally need to be such that layout rounding will
                    // cause drift from actual sizes.
                    (new DockTabNodeViewModel(), 1.0),
                    (new DockTabNodeViewModel(), 2.0),
                    (new DockTabNodeViewModel(), 3.0),
                ];

            DockSplitNodeViewModel viewModel = new(Orientation.Horizontal);

            foreach ((DockTabNodeViewModel content, Double size) in childrenToAdd)
            {
                viewModel.AddChild(content, size);
            }

            List<Double> sizesBeforeLayout = [.. viewModel.Sizes];

            DockSplitPanel panel = new()
            {
                DataContext = viewModel,
            };

            Window window = new() { Content = panel };
            window.Show();

            viewModel.Sizes.Count.ShouldBe(
                childrenToAdd.Count,
                $"Expected {childrenToAdd.Count} normalized sizes to match {childrenToAdd.Count} children");

            viewModel.Sizes.Sum()
                .ShouldBe(1.0, 0.001, "Normalized sizes should sum to 1.0");

            Boolean sizesChanged = !viewModel.Sizes.SequenceEqual(sizesBeforeLayout);

            sizesChanged
                .ShouldBeTrue("Expected updated sizes based on actual layout");
        }
    }
}
