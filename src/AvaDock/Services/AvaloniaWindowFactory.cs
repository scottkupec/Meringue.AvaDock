// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// An implementation of <see cref="IWindowFactory"/> that creates <see cref="AvaloniaWindowAdapter"/> instances.
    /// </summary>
    internal class AvaloniaWindowFactory : IWindowFactory
    {
        /// <summary><see cref="Lazy{T}"/> backing field for the main application window.</summary>
        private readonly Lazy<IWindow?> mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaWindowFactory"/> class.
        /// </summary>
        public AvaloniaWindowFactory()
        {
            this.mainWindow = new Lazy<IWindow?>(() =>
            {
                return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow is Window window
                    ? new AvaloniaWindowAdapter(window)
                    : (IWindow?)null;
            });
        }

        /// <inheritdoc/>
        public IWindow MainWindow => this.mainWindow.Value!;

        /// <inheritdoc/>
        public IWindow CreateWindow()
        {
            Window? main = GetMainWindow();
            WorkspaceWindow window = main != null ? new WorkspaceWindow(main) : new WorkspaceWindow();
            return new AvaloniaWindowAdapter(window);
        }

        /// <summary>
        /// Gets the current application main window, if one is available.
        /// </summary>
        /// <returns>The main window instance, or null when a desktop application lifetime is not active.</returns>
        private static Window? GetMainWindow()
        {
            return Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime
                ? desktopLifetime.MainWindow
                : null;
        }
    }
}
