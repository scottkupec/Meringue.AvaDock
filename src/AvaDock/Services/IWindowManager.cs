// Copyright (C) Scott Kupec. All rights reserved.

using System.Collections.Generic;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Defines the interface for managing <see cref="IWindow"/>s.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Gets the main application <see cref="IWindow"/>.
        /// </summary>
        IWindow? MainWindow { get; }

        /// <summary>
        /// Gets the list of currently managed <see cref="IWindow"/>s.
        /// </summary>
        IReadOnlyList<IWindow> Windows { get; }

        /// <summary>
        /// Creates a new <see cref="IWindow"/>s. instance.
        /// </summary>
        /// <returns>The newly created <see cref="IWindow"/>.</returns>
        IWindow CreateWindow();

        /// <summary>
        /// Calls <see cref="IWindow.Show"/> on all managed <see cref="IWindow"/>s.
        /// </summary>
        void ShowAll();
    }
}
