// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;

namespace HelloDock
{
    /// <summary>Main application entry point.</summary>
    internal sealed class Program
    {
        /// <summary>Application entry point.</summary>
        /// <param name="args">Arguments passed from the host environment.</param>
        // Avalonia configuration, don't remove; also used by visual designer.
        [STAThread]
        public static void Main(String[] args) =>
            AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(args);
    }
}
