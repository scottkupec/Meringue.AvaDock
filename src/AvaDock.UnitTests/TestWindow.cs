// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Meringue.AvaDock.Services;

namespace Meringue.AvaDock.UnitTests
{
    /// <summary>
    /// A test-safe implementation of <see cref="IWindow"/> that avoids Avalonia UI dependencies.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class TestWindow : IWindow
    {
        // Simulate a single screen at (0,0)-(1920,1080)
        private readonly PixelRect screen = new(0, 0, 1920, 1080);

        /// <inheritdoc/>
        public event EventHandler? Closed;

        /// <inheritdoc/>
        public event EventHandler<WindowClosingEventArgs>? Closing;

        /// <inheritdoc/>
        public Rect Bounds { get; set; } = new Rect(0, 0, 100, 100);

        /// <inheritdoc/>
        public Boolean CanResize { get; set; }

        /// <inheritdoc/>
        public Object? Content { get; set; }

        /// <inheritdoc/>
        public Object? DataContext { get; set; }

        /// <inheritdoc/>
        public Double Height { get; set; }

        /// <inheritdoc/>
        public Boolean IsVisible { get; set; }

        /// <inheritdoc/>
        public PixelPoint Position { get; set; }

        /// <inheritdoc/>
        public Boolean ShowInTaskbar { get; set; }

        /// <inheritdoc/>
        public String? Title { get; set; }

        /// <inheritdoc/>
        public Double Width { get; set; }

        /// <inheritdoc/>
        public WindowStartupLocation WindowStartupLocation { get; set; }

        /// <summary>Gets the id of this instance.</summary>
        public String Id { get; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets a collection of child windows owned by this window.
        /// </summary>
        private List<TestWindow> ChildWindows { get; } = [];

        /// <summary>
        /// Gets a collection of child windows owned by this window.
        /// </summary>
        private TestWindow? ParentWindow { get; set; }

        /// <inheritdoc/>
        public void Close()
        {
            this.ParentWindow?.ChildWindows.Remove(this);

            Boolean childCancelledClose = false;

            foreach (TestWindow child in this.ChildWindows.ToList())
            {
                WindowClosingEventArgs childArgs = WindowClosingEventArgsFactory.Create(WindowCloseReason.OwnerWindowClosing, true);
                child.Closing?.Invoke(this, childArgs);

                if (!childArgs.Cancel)
                {
                    child.Closed?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    childCancelledClose = true;
                    break;
                }
            }

            if (!childCancelledClose)
            {
                WindowClosingEventArgs args = WindowClosingEventArgsFactory.Create(WindowCloseReason.WindowClosing, false);
                this.Closing?.Invoke(this, args);

                if (!args.Cancel)
                {
                    this.IsVisible = false;
                    this.Closed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public Boolean IsOnScreen(PixelRect rect)
        {
            return this.screen.Intersects(rect);
        }

        /// <inheritdoc/>
        public void Show(IWindow? owner = null)
        {
            if (owner is TestWindow owningWindow)
            {
                this.ParentWindow = owningWindow;
                owningWindow.ChildWindows.Add(this);
            }

            this.IsVisible = true;
        }

        /// <summary>
        /// Helper class for constructing <see cref="WindowClosingEventArgs"/> which is not currently exposed but necessary for this implementation
        /// to manage parent and child windows.
        /// </summary>
        private static class WindowClosingEventArgsFactory
        {
            public static WindowClosingEventArgs Create(WindowCloseReason reason = WindowCloseReason.WindowClosing, Boolean isProgrammatic = true)
            {
                ConstructorInfo? ctor = typeof(WindowClosingEventArgs)
                    .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(WindowCloseReason), typeof(Boolean)], null) ??
                    throw new InvalidOperationException("Could not find internal constructor for WindowClosingEventArgs.");

                return (WindowClosingEventArgs)ctor!.Invoke([reason, isProgrammatic]);
            }
        }
    }
}
