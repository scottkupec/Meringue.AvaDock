// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;
using Avalonia.Headless;
using Meringue.AvaDock.UnitTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Meringue.AvaDock.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
