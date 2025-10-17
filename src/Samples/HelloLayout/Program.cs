// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Logging;

namespace HelloLayout
{
    /// <summary>Main application entry point.</summary>
    internal sealed class Program
    {
        /// <summary>Application entry point.</summary>
        /// <param name="args">Arguments passed from the host environment.</param>
        // Avalonia configuration, don't remove; also used by visual designer.
        [STAThread]
        public static void Main(String[] args)
        {
            _ = AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .LogToTrace(LogEventLevel.Error, LogArea.Win32Platform, LogArea.Layout, LogArea.Visual, LogArea.Control)
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
