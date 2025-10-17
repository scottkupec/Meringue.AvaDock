// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Meringue.AvaDock.UnitTests;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.ViewModels.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockSplitNodeViewModelTests
    {
        [Fact]
        public void AddChild_PreservesSizes()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal();

            List<Double> inputSizes = [2.0, 3.0, 5.0];

            for (Int32 initialChildCount = 0; initialChildCount < inputSizes.Count; initialChildCount++)
            {
                Int32 newChildCount = initialChildCount + 1;

                DockTabNodeViewModel child = new();
                split.AddChild(child, inputSizes[initialChildCount]);

                split.Sizes.Count
                    .ShouldBe(newChildCount, $"Expected {newChildCount} sizes after adding {newChildCount} children.");

                Double givenSize = inputSizes[initialChildCount];

                split.Sizes[initialChildCount]
                    .ShouldBe(givenSize, $"Expected size of child {newChildCount} to be {givenSize}.");
            }
        }

        [Fact]
        public void GetChildAt_ReturnsCorrectChild()
        {
            DockTabNodeViewModel tabNode1 = new();
            DockTabNodeViewModel tabNode2 = new();
            DockSplitNodeViewModel split = DockTree.Horizontal(tabNode1, tabNode2);

            split.GetChildAt(0)
                .ShouldBe(tabNode1, $"{nameof(DockSplitNodeViewModel.GetChildAt)} should return {nameof(tabNode1)} at index 0.");

            split.GetChildAt(1)
                .ShouldBe(tabNode2, $"{nameof(DockSplitNodeViewModel.GetChildAt)} should return {nameof(tabNode2)} at index 1.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void GetChildAt_ThrowsForInvalidIndex(Int32 index)
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab(),
                DockTree.Tab());

            Should.Throw<ArgumentOutOfRangeException>(
                () => split.GetChildAt(index),
                $"{nameof(DockSplitNodeViewModel.GetChildAt)} should throw for index {index} when child count is {split.Children.Count}.");
        }

        [Fact]
        public void IndexOf_ReturnsCorrectIndex()
        {
            DockTabNodeViewModel tabNode1 = new();
            DockTabNodeViewModel tabNode2 = new();
            DockTabNodeViewModel notPresentTabNode = new();

            DockSplitNodeViewModel split = DockTree.Horizontal(tabNode1, tabNode2);

            split.IndexOf(tabNode1)
                .ShouldBe(0, $"{nameof(tabNode1)} should be at index 0");

            split.IndexOf(tabNode2)
                .ShouldBe(1, $"{nameof(tabNode2)} should be at index 1");

            split.IndexOf(notPresentTabNode)
                .ShouldBe(-1, $"{nameof(notPresentTabNode)} should not have an index.");
        }

        [Fact]
        public void InsertAt_InsertsChildAtIndex()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab("item-1"),
                DockTree.Tab("item-2"));

            Double insertedSize = 10;
            DockTabNodeViewModel insertedChild = new();

            split.InsertAt(1, insertedChild, insertedSize);

            split.Children[1]
                .ShouldBe(insertedChild, $"Expected {nameof(insertedChild)} at {nameof(DockSplitNodeViewModel.Children)} index 1.");

            split.Sizes.Count
                .ShouldBe(3, $"Expected 3 sizes after {nameof(DockSplitNodeViewModel.InsertAt)}.");

            split.Sizes[1]
                .ShouldBe(insertedSize, $"Expected {nameof(insertedSize)} at {nameof(DockSplitNodeViewModel.Sizes)} index 1.");
        }

        [Fact]
        public void RemoveChild_RemovesChildAndRetainsSizes()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab("item-1"),
                DockTree.Tab("item-2"),
                DockTree.Tab("item-3"));

            split.UpdateSizes([2.0, 3.0, 5.0]);

            split.RemoveChild(split.Children[1]);

            split.Children.Count
                .ShouldBe(2, "Expected 2 children after removal");

            split.Sizes.Count
                .ShouldBe(2, "Expected 2 sizes after removal");

            split.Sizes.Sum()
                .ShouldBe(7.0, "Sizes should sum to 7.0");
        }

        [Fact]
        public void RemoveEmptyPanels_DoesNotRemoveTabWithTabs()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab("item"));

            split.Children.Count
                .ShouldBe(1, $"Sanity: The {nameof(DockSplitNodeViewModel)} being tested must contain children.");

            split.RemoveEmptyPanels();

            split.Children.Count
                .ShouldBe(1, $"A {nameof(DockTabNodeViewModel)} with tabs should not be removed.");
        }

        [Fact]
        public void RemoveEmptyPanels_PromotesSingleNonEmptyChild()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Vertical(
                    DockTree.Tab("item")));

            split.Children.OfType<DockSplitNodeViewModel>()
                .ShouldNotBeEmpty("Sanity: At least one nested split must exist.");

            split.Children.OfType<DockTabNodeViewModel>()
                .ShouldBeEmpty("Sanity: No direct tab node is expected to exist..");

            split.RemoveEmptyPanels();

            split.Children.OfType<DockSplitNodeViewModel>()
                .ShouldBeEmpty("Single-child split should be removed.");

            split.Children.OfType<DockTabNodeViewModel>()
                .ShouldNotBeEmpty("Non-empty tab node should be promoted to parent after removing single-child split.");
        }

        [Fact]
        public void RemoveEmptyPanels_RemovesEmptySplitNode()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Vertical());

            split.Children
                .ShouldNotBeEmpty($"Sanity: The {nameof(DockSplitNodeViewModel)} being tested must contain children.");

            split.RemoveEmptyPanels();

            split.Children
                .ShouldBeEmpty($"Empty {nameof(DockSplitNodeViewModel)} should have been removed.");
        }

        [Fact]
        public void RemoveEmptyPanels_RemovesEmptyTabNode()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Tab());

            split.Children
                .ShouldNotBeEmpty($"Sanity: The {nameof(DockSplitNodeViewModel)} being tested must contain children.");

            split.RemoveEmptyPanels();

            split.Children
                .ShouldBeEmpty($"Empty {nameof(DockTabNodeViewModel)} should have been removed.");
        }

        [Fact]
        public void RemoveEmptyPanels_RemovesMultipleEmptyChildren()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Vertical(),
                DockTree.Vertical(
                    DockTree.Horizontal(
                        DockTree.Tab())));

            split.Children
                .ShouldNotBeEmpty($"Sanity: The {nameof(DockSplitNodeViewModel)} being tested must contain children.");

            split.RemoveEmptyPanels();

            split.Children
                .ShouldBeEmpty($"Empty {nameof(DockSplitNodeViewModel)} should have been removed.");
        }

        [Fact]
        public void ReplaceChildAt_ReplacesChild()
        {
            Double size = 10;
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab("item-1"));

            split.UpdateSizes([size]);

            DockTabNodeViewModel replacement = new();

            split.ReplaceChildAt(0, replacement);

            split.Children[0]
                .ShouldBe(replacement, $"Expected {nameof(replacement)} at index 0");

            split.Sizes[0]
                .ShouldBe(size, "Size should remain same after replacement");
        }

        [Fact]
        public void UpdateSizes_ReplacesSizes()
        {
            DockSplitNodeViewModel split = DockTree.Horizontal(
                DockTree.Tab("item-1"),
                DockTree.Tab("item-2"),
                DockTree.Tab("item-3"));

            split.UpdateSizes([2.0, 3.0, 5.0]);

            split.Sizes.Count
                .ShouldBe(3, "Expected 3 sizes after update");

            split.Sizes.Sum()
                .ShouldBe(10.0, 0.001, "Sizes should be normalized to sum to 1.0");

            split.Sizes[0]
                .ShouldBe(2.0, 0.001, "First size should be 2/10 = 0.2");

            split.Sizes[1]
                .ShouldBe(3.0, 0.001, "Second size should be 3/10 = 0.3");

            split.Sizes[2]
                .ShouldBe(5.0, 0.001, "Third size should be 5/10 = 0.5");
        }
    }
}
