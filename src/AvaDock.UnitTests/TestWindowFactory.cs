// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.Services;

namespace Meringue.AvaDock.UnitTests
{
    /// <summary>
    /// A test-safe implementation of <see cref="IWindowFactory"/> that returns <see cref="TestWindow"/> instances.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class TestWindowFactory : IWindowFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestWindowFactory"/> class.
        /// </summary>
        public TestWindowFactory()
        {
            this.MainWindow = new TestWindow
            {
                Height = 600,
                IsVisible = true,
                ShowInTaskbar = true,
                Width = 800,
            };
        }

        /// <inheritdoc/>
        public IWindow MainWindow { get; }

        /// <inheritdoc/>
        public IWindow CreateWindow() => new TestWindow();
    }
}
