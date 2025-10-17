// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Meringue.AvaDock.UnitTests;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.ViewModels.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockNodeViewModelTests
    {
        [Fact]
        public void FindOwningTab_ReturnsCorrectTabNode()
        {
            String itemId = "item";
            DockSplitNodeViewModel split = DockTree.Horizontal(DockTree.Vertical(DockTree.Tab(itemId)));

            split.FindOwningTabNode(itemId)
                .ShouldNotBeNull($"{nameof(DockSplitNodeViewModel.FindOwningTabNode)} should return the tab node that contains the item.");
        }
    }
}
