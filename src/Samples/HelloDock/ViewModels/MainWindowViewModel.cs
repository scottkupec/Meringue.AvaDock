// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace HelloDock.ViewModels
{
    /// <summary>
    /// The main view model that controls which view is currently displayed in the application.
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
        public MainWindowViewModel()
        {
            this.DockManager = BuildHostRoot();
        }

        /// <summary>Gets the thing.</summary>
        public DockControlManager DockManager { get; }

        /// <summary>Build a DockHostManager.</summary>.
        /// <returns>The thing built.</returns>
        private static DockControlManager BuildHostRoot()
        {
            DockSplitNodeViewModel splitPanel = new(Orientation.Horizontal);
            DockTabNodeViewModel helloTabPanel = new();

            helloTabPanel.AddTab(
                new DockItemViewModel()
                {
                    Title = "Hello Item",
                    Context = new TextBlock { Text = "Hello", Margin = new Thickness(8) },
                });

            DockTabNodeViewModel dockTabPanel = new();

            dockTabPanel.AddTab(
                new DockItemViewModel()
                {
                    Title = "Dock Item",
                    Context = new TextBlock { Text = "Dock", Margin = new Thickness(8) },
                });

            splitPanel.AddChild(helloTabPanel);
            splitPanel.AddChild(dockTabPanel);

            return new DockControlManager(new DockWorkspaceManager(splitPanel));
        }
    }
}
