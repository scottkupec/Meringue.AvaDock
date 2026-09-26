// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meringue.AvaDock.Events;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace LayoutMonitor.ViewModels
{
    /// <summary>
    /// The main view model that controls which view is currently displayed in the application.
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the status message showing layout changes.
        /// </summary>
        [ObservableProperty]
        private String statusMessage = $"[{MainWindowViewModel.Now}] Layout monitoring initialized";

        /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
        public MainWindowViewModel()
        {
            this.LayoutManager = BuildLayoutRoot();

            // Subscribe to layout change events
            this.SetupLayoutMonitoring();
        }

        /// <summary>
        /// Gets the layout manager.
        /// </summary>
        public DockLayoutManager<CustomToolViewModel> LayoutManager { get; }

        /// <summary>
        /// Gets "Now" as a string because I was tired of copying the full format around.
        /// </summary>
        private static String Now => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);

        /// <summary>
        /// Builds the initial layout.
        /// </summary>
        /// <returns>The layout manager.</returns>
        private static DockLayoutManager<CustomToolViewModel> BuildLayoutRoot()
        {
            // Create the layout manager with 3 top level splits.
            DockLayoutManager<CustomToolViewModel> layout = new("left", "center", "right");

            // Add an item to the left panel
            _ = layout.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "1",
                    Title = "Left Item",
                    Context = new TextBlock { Text = "Left" },
                },
                preferredTabPanelId: "left");

            DockItemViewModel? cantCloseTool = layout.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "2",
                    Title = "Center Item",
                    Context = new TextBlock { Text = "Center" },
                },
                preferredTabPanelId: "center");

            cantCloseTool!.DisableClose = true;
            cantCloseTool!.DisableHide = true;

            // Add two items to the right tab panel and stack them
            _ = layout.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "3",
                    Title = "Right Item 1",
                    Context = new TextBlock { Text = "Right 1" },
                },
                preferredTabPanelId: "right");

            _ = layout.DockControl.AddItem(
                new DockItemViewModel()
                {
                    Id = "4",
                    Title = "Right Item 2",
                    Context = new TextBlock { Text = "Right 2" },
                },
                preferredTabPanelId: "right");

            return layout;
        }

        /// <summary>
        /// Adds a new item to demonstrate layout changes.
        /// </summary>
        [RelayCommand]
        private void AddItem()
        {
            DockItemViewModel? newItem = this.LayoutManager.DockControl.AddItem(
                new CustomToolViewModel()
                {
                    Id = $"item_{DateTime.Now.Ticks}",
                    Title = "New Item",
                    Context = new TextBlock { Text = $"Added item at {MainWindowViewModel.Now}" },
                },
                preferredTabPanelId: "right");

            this.StatusMessage = $"[{MainWindowViewModel.Now}] Added new item: {newItem?.Title}";
        }

        /// <summary>
        /// Load the saved layout.
        /// </summary>
        [RelayCommand]
        private void LoadLayout()
        {
            _ = this.LayoutManager.LoadLayout("layout.json");
            this.StatusMessage = $"[{MainWindowViewModel.Now}] Layout loaded";
        }

        /// <summary>
        /// Handles items collection changes in the workspace.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="NotifyCollectionChangedEventArgs"/> for the event.</param>
        private void OnItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            String action = eventArgs.Action switch
            {
                NotifyCollectionChangedAction.Add => "Added",
                NotifyCollectionChangedAction.Remove => "Removed",
                NotifyCollectionChangedAction.Replace => "Replaced",
                NotifyCollectionChangedAction.Move => "Moved",
                NotifyCollectionChangedAction.Reset => "Reset",
                _ => "Unknown",
            };

            this.StatusMessage = $"[{MainWindowViewModel.Now}] Items collection changed: {action} {eventArgs.NewItems?.Count ?? eventArgs.OldItems?.Count} item(s)";
        }

        /// <summary>
        /// Handles minimized items collection changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="NotifyCollectionChangedEventArgs"/> for the event.</param>
        private void OnMinimizedItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            String action = eventArgs.Action switch
            {
                NotifyCollectionChangedAction.Add => "Added",
                NotifyCollectionChangedAction.Remove => "Removed",
                NotifyCollectionChangedAction.Replace => "Replaced",
                NotifyCollectionChangedAction.Move => "Moved",
                NotifyCollectionChangedAction.Reset => "Reset",
                _ => "Unknown",
            };

            this.StatusMessage = $"[{MainWindowViewModel.Now}] Minimized items collection changed: {action} {eventArgs.NewItems?.Count ?? eventArgs.OldItems?.Count} item(s)";
        }

        /// <summary>
        /// Handles workspace attachment events.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DockWorkspaceAttachedEventArgs"/> for the event.</param>
        private void OnWorkspaceAttached(Object? sender, DockWorkspaceAttachedEventArgs eventArgs)
        {
            this.StatusMessage = $"[{MainWindowViewModel.Now}] Workspace attached: {eventArgs.Workspace.Id}";
        }

        /// <summary>
        /// Handles workspace detachment events.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DockWorkspaceDetachedEventArgs"/> for the event.</param>
        private void OnWorkspaceDetached(Object? sender, DockWorkspaceDetachedEventArgs eventArgs)
        {
            this.StatusMessage = $"[{MainWindowViewModel.Now}] Workspace detached: {eventArgs.Workspace.Id}";
        }

        /// <summary>
        /// Handles property changes on the workspace.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="PropertyChangedEventArgs"/> for the event.</param>
        private void OnWorkspacePropertyChanged(Object? sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(DockWorkspaceManager.DockTree))
            {
                this.StatusMessage = $"[{MainWindowViewModel.Now}] Dock tree structure changed";
                // Here you could also re-subscribe to events on the new dock tree if needed
            }
        }

        /// <summary>
        /// Save the layout.
        /// </summary>
        [RelayCommand]
        private void SaveLayout()
        {
            this.LayoutManager.SaveLayout("layout.json");
            this.StatusMessage = $"[{MainWindowViewModel.Now}] Layout saved";
        }

        /// <summary>
        /// Sets up monitoring for layout changes.
        /// </summary>
        private void SetupLayoutMonitoring()
        {
            // Subscribe to the ItemsChanged event on the primary workspace
            DockWorkspaceManager workspace = this.LayoutManager.DockControl.PrimaryWorkspace;
            if (workspace != null)
            {
                workspace.ItemsChanged += this.OnItemsChanged;
                workspace.MinimizedItemsChanged += this.OnMinimizedItemsChanged;

                // Also subscribe to changes in the dock tree structure
                // Note: We can monitor the root dock tree for changes through the WorkspaceManager's DockTree
                if (workspace.DockTree != null)
                {
                    // This will get called any time the workspace tree structure changes
                    workspace.PropertyChanged += this.OnWorkspacePropertyChanged;
                }
            }

            this.StatusMessage = $"[{MainWindowViewModel.Now}] Monitoring layout changes...";

            // Subscribe to workspace attachment and detachment events
            this.LayoutManager.DockControl.WorkspaceAttached += this.OnWorkspaceAttached;
            this.LayoutManager.DockControl.WorkspaceDetached += this.OnWorkspaceDetached;
        }
    }
}
