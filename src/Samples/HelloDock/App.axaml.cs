// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HelloDock.Windows;

namespace HelloDock
{
    /// <summary>The Avalonia application.</summary>
    public partial class App : Application
    {
        /// <inheritdoc/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    _ = System.Diagnostics.Debugger.Launch();
                }
            };
#endif
        }

        /// <inheritdoc/>
        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
