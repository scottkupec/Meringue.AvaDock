// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;
using Avalonia.Markup.Xaml;

namespace Meringue.AvaDock.UnitTests
{
    /// <summary>The Avalonia application.</summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public partial class TestApplication : Application
    {
        /// <inheritdoc/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
