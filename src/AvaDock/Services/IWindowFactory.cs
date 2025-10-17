// Copyright (C) Scott Kupec. All rights reserved.

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Defines a factory for creating <see cref="IWindow"/> instances.
    /// </summary>
    public interface IWindowFactory
    {
        /// <summary>Gets the main application window.</summary>
        IWindow MainWindow { get; }

        /// <summary>
        /// Creates a new <see cref="IWindow"/> instance.
        /// </summary>
        /// <returns>The newly created <see cref="IWindow"/>.</returns>
        IWindow CreateWindow();
    }
}
