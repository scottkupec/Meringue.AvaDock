// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Headless.XUnit;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    // TODO: Render() tests once updated to Avalonia 12 and DrawingContext is more testable.
    public sealed class TabReorderAdornerTests
    {
        [AvaloniaFact]
        public void SetVisible_UpdatesIsVisible()
        {
            TabReorderAdorner adorner = new();

            adorner.SetVisible(true);
            adorner.IsVisible
                .ShouldBeTrue("SetVisible(true) should set IsVisible to true.");

            adorner.SetVisible(false);
            adorner.IsVisible
                .ShouldBeFalse("SetVisible(false) should set IsVisible to false.");
        }
    }
}
