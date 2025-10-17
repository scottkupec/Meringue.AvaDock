// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Manages <see cref="IWindow"/>s so that they follow the lifecycle of the <see cref="IWindow"/>
    /// hosting the <see cref="DockControl"/>. This includes activation, minimization, restoration, and closure events.
    /// </summary>
    /// <remarks>
    /// Child windows follow the parent window's minimize, restore, close lifecycle.
    /// </remarks>
    internal sealed class WindowManager : IWindowManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManager"/> class.
        /// </summary>
        public WindowManager()
            : this(new AvaloniaWindowFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManager"/> class.
        /// </summary>
        /// <param name="windowFactory">
        /// The <see cref="IWindowFactory"/> to use to create new <see cref="IWindow"/>s.
        /// </param>
        public WindowManager(IWindowFactory windowFactory)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(windowFactory);
            this.WindowFactory = windowFactory;
        }

        /// <summary>Gets the top-level <see cref="IWindow"/>.</summary>
        public IWindow? MainWindow => this.WindowFactory.MainWindow;

        /// <summary>Gets the list of <see cref="IWindow"/>s being managed.</summary>
        /// <remarks>This does not include <see cref="MainWindow"/> which is managed by the runtime.</remarks>
        public IReadOnlyList<IWindow> Windows => this.ChildWindows;

        /// <summary>Gets the list of <see cref="IWindow"/>s being managed.</summary>
        private List<IWindow> ChildWindows { get; } = [];

        /// <summary>Gets the <see cref="IWindowFactory"/> for creating <see cref="IWindow"/> instances.</summary>
        private IWindowFactory WindowFactory { get; }

        /// <summary>
        /// Creates a new <see cref="IWindow"/> and registers it with the current instance.
        /// </summary>
        /// <returns>The <see cref="IWindow"/> created.</returns>
        public IWindow CreateWindow()
        {
            IWindow child = this.WindowFactory.CreateWindow();
            child.Title = this.MainWindow?.Title;
            this.RegisterChildWindow(child);
            return child;
        }

        /// <summary>
        /// Removes a <see cref="IWindow"/> from the current instance.
        /// </summary>
        /// <param name="window">The <see cref="IWindow"/> to be removed.</param>
        public void RemoveWindow(IWindow window)
        {
            this.UnregisterChildWindow(window);
        }

        /// <summary>
        /// Shows all <see cref="IWindow"/>s associated with the current instance.
        /// </summary>
        // TODO: Consider a refactor so this isn't needed.  It exists for when we're loading
        //       a layout and the dock control hasn't yet been attached to the visual tree.
        public void ShowAll()
        {
            foreach (IWindow child in this.ChildWindows.ToList())
            {
                child.Show(this.MainWindow!);
            }
        }

        /// <summary>
        /// Handles closure of a <see cref="IWindow"/> by automatically unregistering it.
        /// </summary>
        /// <param name="sender">The <see cref="IWindow"/> instance.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnChildClosed(Object? sender, EventArgs eventArgs)
        {
            if (sender is IWindow window)
            {
                this.UnregisterChildWindow(window);
            }
        }

        /// <summary>
        /// Registers a <see cref="IWindow"/> with the manager so that it follows <see cref="MainWindow"/>’s lifecycle.
        /// </summary>
        /// <param name="window">The child <see cref="IWindow"/> to register.</param>
        private void RegisterChildWindow(IWindow window)
        {
            if (window is not null && !this.ChildWindows.Contains(window))
            {
                this.ChildWindows.Add(window);
                window.Closed += this.OnChildClosed;
            }
        }

        /// <summary>
        /// Unregisters a <see cref="IWindow"/> so that it no longer follows <see cref="MainWindow"/>'s lifecycle.
        /// </summary>
        /// <param name="window">The child <see cref="IWindow"/> to unregister.</param>
        private void UnregisterChildWindow(IWindow window)
        {
            if (window is not null && this.ChildWindows.Remove(window))
            {
                window.Closed -= this.OnChildClosed;
            }
        }
    }
}
