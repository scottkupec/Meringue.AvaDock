// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock;
using Meringue.AvaDock.Managers;

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
            this.DockManager = DockTree.Control(
                DockTree.Workspace(
                    DockTree.Horizontal(
                        DockTree.Tab(
                            DockTree.Item(
                                id: "hello-id",
                                title: "Hello Item",
                                context: new TextBlock { Text = "Hello", Margin = new Thickness(8) })),
                        DockTree.Tab(
                            DockTree.Item(
                                id: "dock-id",
                                title: "Dock Item",
                                context: new TextBlock { Text = "Dock", Margin = new Thickness(8) })))));
        }

        /// <summary>Gets the <see cref="DockControlManager"/> managing the dock control instance.</summary>
        public DockControlManager DockManager { get; }
    }
}
