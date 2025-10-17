// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Represents a window in the docking system.
    /// </summary>
    /// <remarks>
    /// Based on the necessary members of <see cref="Window"/> in order to abstract that away in tests.
    /// </remarks>
    public interface IWindow
    {
        /// <summary>
        /// Occurs after the window is closed.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Occurs when the window is closing.
        /// </summary>
        event EventHandler<WindowClosingEventArgs>? Closing;

        /// <summary>
        /// Gets the window's bounds.
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// Gets or sets the data context associated with the window.
        /// </summary>
        Object? DataContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the window can be resized by the user.
        /// </summary>
        Boolean CanResize { get; set; }

        /// <summary>
        /// Gets or sets the content displayed in the window.
        /// </summary>
        Object? Content { get; set; }

        /// <summary>
        /// Gets or sets the window's height.
        /// </summary>
        Double Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the window is currently visible.
        /// </summary>
        Boolean IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the window's position on screen.
        /// </summary>
        PixelPoint Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the window is shown in the taskbar.
        /// </summary>
        Boolean ShowInTaskbar { get; set; }

        /// <summary>
        /// Gets or sets the title of the window.
        /// </summary>
        String? Title { get; set; }

        /// <summary>
        /// Gets or sets the window's width.
        /// </summary>
        Double Width { get; set; }

        /// <summary>
        /// Gets or sets the window's startup location.
        /// </summary>
        WindowStartupLocation WindowStartupLocation { get; set; }

        /// <summary>
        /// Closes the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Determines whether the specified rectangle intersects any visible screen.
        /// </summary>
        /// <param name="rect">The pixel rectangle to test.</param>
        /// <returns><c>true</c> if the rectangle intersects a screen; otherwise, <c>false</c>.</returns>
        Boolean IsOnScreen(PixelRect rect);

        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <param name="owner">The optional owner window.</param>
        void Show(IWindow? owner = null);
    }
}
