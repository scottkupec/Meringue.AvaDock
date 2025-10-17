// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Wraps an Avalonia <see cref="Avalonia.Controls.Window"/> to expose it via the <see cref="IWindow"/> interface.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage] // Worth testing?
    public sealed class AvaloniaWindowAdapter : IWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvaloniaWindowAdapter"/> class.
        /// </summary>
        /// <param name="window">The <see cref="Avalonia.Controls.Window"/> to adapt.</param>
        public AvaloniaWindowAdapter(Window window)
        {
            this.Window = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <inheritdoc/>
        public event EventHandler? Closed
        {
            add => this.Window.Closed += value;
            remove => this.Window.Closed -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<WindowClosingEventArgs>? Closing
        {
            add => this.Window.Closing += value;
            remove => this.Window.Closing -= value;
        }

        /// <inheritdoc/>
        public Rect Bounds => this.Window.Bounds;

        /// <inheritdoc/>
        public Boolean CanResize
        {
            get => this.Window.CanResize;
            set => this.Window.CanResize = value;
        }

        /// <inheritdoc/>
        public Object? Content
        {
            get => this.Window.Content;
            set => this.Window.Content = value;
        }

        /// <inheritdoc/>
        public Object? DataContext
        {
            get => this.Window.DataContext;
            set => this.Window.DataContext = value;
        }

        /// <inheritdoc/>
        public Double Height
        {
            get => this.Window.Height;
            set => this.Window.Height = value;
        }

        /// <inheritdoc/>
        public Boolean IsVisible
        {
            get => this.Window.IsVisible;
            set => this.Window.IsVisible = value;
        }

        /// <inheritdoc/>
        public PixelPoint Position
        {
            get => this.Window.Position;
            set => this.Window.Position = value;
        }

        /// <inheritdoc/>
        public Boolean ShowInTaskbar
        {
            get => this.Window.ShowInTaskbar;
            set => this.Window.ShowInTaskbar = value;
        }

        /// <inheritdoc/>
        public String? Title
        {
            get => this.Window.Title;
            set => this.Window.Title = value;
        }

        /// <inheritdoc/>
        public Double Width
        {
            get => this.Window.Width;
            set => this.Window.Width = value;
        }

        /// <inheritdoc/>
        public WindowStartupLocation WindowStartupLocation
        {
            get => this.Window.WindowStartupLocation;
            set => this.Window.WindowStartupLocation = value;
        }

        /// <summary>Gets the <see cref="Avalonia.Controls.Window"/> being adapted to <see cref="IWindow"/>.</summary>
        private Window Window { get; }

        /// <inheritdoc/>
        public void Close() => this.Window.Close();

        /// <inheritdoc/>
        public Boolean IsOnScreen(PixelRect rect)
        {
            foreach (Screen screen in this.Window.Screens.All)
            {
                if (screen.WorkingArea.Intersects(rect))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void Show(IWindow? owner = null)
        {
            AvaloniaWindowAdapter? implementation = owner as AvaloniaWindowAdapter;

            System.Diagnostics.Debug.Assert(
                implementation is not null,
                $"Mixing {nameof(IWindow)} implementations is not supported.");

            if (implementation is not null)
            {
                this.Window.Show(implementation.Window);
            }
            else
            {
                this.Window.Show();
            }
        }
    }
}
