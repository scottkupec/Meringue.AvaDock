// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;

namespace LayoutMonitor
{
    /// <summary>
    /// The main program class for the application.  This particular application is a sample that demonstrates
    /// how to monitor layout changes in LayoutManager.DockControl within a Avalonia application so that the
    /// application can respond to layout changes for tasks such as persistnig those changes to disk.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(System.String[] args)
        {
            _ = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}
