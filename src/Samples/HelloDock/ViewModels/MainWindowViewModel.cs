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

        /// <summary>Build a DockControlManager.</summary>.
        /// <returns>The new <see cref="DockControlManager"/>.</returns>
        /// <remarks>
        /// Builds our initial control layout in memory so we don't need to
        /// issue re-order commands at startup.
        /// </remarks>
        private static DockControlManager BuildHostRoot()
        {
            // Create the intial split panel which will serve as the root of the
            // DockTree for the initial workspace.
            DockSplitNodeViewModel splitPanel = MainWindowViewModel.BuildInitialControlLayout();

            // Pass the DockTree to a new DockWorkspaceManager which will manage
            // the item state changes.
            DockWorkspaceManager workspace = new(splitPanel);

            // Use the new workspace as the primary workspace in a new DockControlManager
            // instance which enables floating (move a new workspace).
            return new DockControlManager(workspace);
        }

        /// <summary>Build a DockSplitNodeViewModel.</summary>.
        /// <returns>The new <see cref="DockWorkspaceManager"/>.</returns>
        /// <remarks>
        /// Builds a workspace in memory that is:
        ///   root-split-node
        ///     +--- first tab node
        ///     | +--- a dock item with the text "Hello"
        ///     +--- second tab node
        ///       +--- a dock item with the text "Dock"
        /// using a horizontal layout so the dock items appear beside each other.
        /// </remarks>
        private static DockSplitNodeViewModel BuildInitialControlLayout()
        {
            // Root split node that lays out children horizontally.
            DockSplitNodeViewModel splitPanel = new(Orientation.Horizontal);

            // First tab node with a single child containing the text "Hello"
            DockTabNodeViewModel helloTabPanel = new();
            helloTabPanel.AddTab(
                new DockItemViewModel()
                {
                    Title = "Hello Item",
                    Context = new TextBlock { Text = "Hello", Margin = new Thickness(8) },
                });

            // First tab node with a single child containing the text "Dock"
            DockTabNodeViewModel dockTabPanel = new();
            dockTabPanel.AddTab(
                new DockItemViewModel()
                {
                    Title = "Dock Item",
                    Context = new TextBlock { Text = "Dock", Margin = new Thickness(8) },
                });

            // Add both child nodes to the root split in the left-to-right order we want
            // them to appear.
            splitPanel.AddChild(helloTabPanel);
            splitPanel.AddChild(dockTabPanel);

            return splitPanel;
        }
    }
}
