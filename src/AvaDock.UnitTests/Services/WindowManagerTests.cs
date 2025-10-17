// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.UnitTests;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Services.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class WindowManagerTests
    {
        [Fact]
        public void ClosedWindow_ShouldBeUnregistered()
        {
            WindowManager manager = new(new TestWindowFactory());

            IWindow window = manager.CreateWindow();
            manager.Windows
                .ShouldContain(window, "Window should be registered before closing");

            window.Close();

            manager.Windows
                .ShouldNotContain(window, "Closed window should be unregistered");
        }

        [Fact]
        public void CreateWindow_ShouldRegisterChildWindow()
        {
            WindowManager manager = new(new TestWindowFactory());

            IWindow window = manager.CreateWindow();

            manager.Windows
                .ShouldContain(window, $"{nameof(WindowManager.CreateWindow)} should register the new window");

            window.IsVisible
                .ShouldBeFalse("Newly created window should not be visible until shown");
        }

        [Fact]
        public void MainWindow_ShouldMatchFactoryMainWindow()
        {
            TestWindowFactory factory = new();
            WindowManager manager = new(factory);

            manager.MainWindow
                .ShouldBe(factory.MainWindow, $"{nameof(WindowManager.ShowAll)} should return the {nameof(IWindowFactory.MainWindow)}");
        }

        [Fact]
        public void ShowAll_ShouldMakeAllWindowsVisible()
        {
            WindowManager manager = new(new TestWindowFactory());

            IWindow window1 = manager.CreateWindow();
            IWindow window2 = manager.CreateWindow();

            manager.ShowAll();

            window1.IsVisible
                .ShouldBeTrue($"{nameof(WindowManager.ShowAll)} should make '{nameof(window1)}' visible");

            window2.IsVisible
                .ShouldBeTrue($"{nameof(WindowManager.ShowAll)} should make '{nameof(window1)}' visible");
        }
    }
}
