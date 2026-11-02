// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meringue.AvaDock;
using Meringue.AvaDock.Managers;
using ThemeTest.Controls;

namespace ThemeTest.ViewModels
{
    /// <summary>The main view model that controls which view is currently displayed in the application.</summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>The window whose theme this view model toggles.</summary>
        private readonly Window targetWindow;

        /// <summary>Gets or sets a value indicating whether the dark theme variant is active.</summary>
        [ObservableProperty]
        private Boolean isDark;

        /// <summary>Indicates whether the floating gallery workspace has already been created.</summary>
        private Boolean floatingGalleryOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="window">The window whose theme this view model toggles.</param>
        public MainWindowViewModel(Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            this.targetWindow = window;
            this.isDark = window.RequestedThemeVariant == ThemeVariant.Dark;

            this.DockManager = DockTree.Control(
                DockTree.Workspace(
                    DockTree.Horizontal(
                        DockTree.Tab(
                            DockTree.Item(
                                context: new ControlGallery(),
                                id: "gallery-dock",
                                title: "Gallery")))));
        }

        /// <summary>Gets the <see cref="DockControlManager"/> managing the dock control instance.</summary>
        public DockControlManager DockManager { get; }

        /// <summary>Opens a floating secondary workspace hosting a <see cref="ControlGallery"/>.</summary>
        public void OpenFloatingGallery()
        {
            if (this.floatingGalleryOpen)
            {
                return;
            }

            this.floatingGalleryOpen = true;

            DockWorkspaceManager workspace = DockTree.Workspace(
                DockTree.Horizontal(
                    DockTree.Tab(
                        DockTree.Item(
                            context: new ControlGallery(),
                            id: "gallery-float",
                            title: "Gallery (Float)"))));

            (PixelPoint? location, Size size) = this.GetWorkspacePositionAndSize();

            _ = this.DockManager.AttachSecondaryWorkspace(workspace, location, size);
            this.DockManager.ShowAllWindows();
        }

        /// <summary>
        /// Gets the position and size for the secondary workspace.
        /// </summary>
        /// <returns> A tuple consisting of:
        /// <list type="bullet">
        /// <item>The <see cref="PixelPoint"/> for the upper left corner of the workspace.</item>
        /// <item>The <see cref="Size"/> of the workspace.</item>
        /// </list>
        /// </returns>
        private (PixelPoint? Center, Size Size) GetWorkspacePositionAndSize()
        {
            Double scale = this.targetWindow.RenderScaling;
            Double centerDipX = this.targetWindow.Bounds.X + (this.targetWindow.Bounds.Width / 2.0);
            Double centerDipY = this.targetWindow.Bounds.Y + (this.targetWindow.Bounds.Height / 2.0);
            PixelPoint center = new((Int32)(centerDipX * scale), (Int32)(centerDipY * scale));

            // Find the monitor that hosts the main window.
            Screen monitor = this.targetWindow.Screens.All[0];
            foreach (Screen screen in this.targetWindow.Screens.All)
            {
                if (screen.WorkingArea.Contains(center))
                {
                    monitor = screen;
                    break;
                }
            }

            PixelSize monitorSize = monitor.Bounds.Size;
            PixelPoint monitorOrigin = monitor.Bounds.Position;
            Double width = monitorSize.Width / 2.0;
            Size size = new(width, monitorSize.Height);

            Double rightEdge = (this.targetWindow.Bounds.Right * scale) + 8.0;
            Double left = (rightEdge + width <= monitorOrigin.X + monitorSize.Width) ? rightEdge : monitorOrigin.X;
            PixelPoint location = new((Int32)left, monitorOrigin.Y);

            return (location, size);
        }

        /// <summary>Toggles the window theme between the dark and light variants.</summary>
        [RelayCommand]
        private void ToggleTheme() => this.targetWindow.RequestedThemeVariant = this.IsDark ? ThemeVariant.Light : ThemeVariant.Dark;
    }
}
