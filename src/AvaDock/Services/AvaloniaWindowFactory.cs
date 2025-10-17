// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

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
        public IWindow CreateWindow() =>
            new AvaloniaWindowAdapter(new Window());
    }
}
