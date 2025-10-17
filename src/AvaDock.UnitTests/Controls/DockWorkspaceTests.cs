// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Headless.XUnit;
using Shouldly;

namespace Meringue.AvaDock.Controls.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DockWorkspaceTests
    {
        [AvaloniaFact]
        public void DataContext_NullDoesNotCrash()
        {
            DockWorkspace control = new()
            {
                DataContext = null,
            };

            Should.NotThrow(control.ApplyTemplate, "Applying template with null DataContext should not throw");
        }
    }
}
