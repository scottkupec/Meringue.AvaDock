// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private String statusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Layout monitoring initialized";

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
        /// Builds the initial layout.
        /// </summary>
        /// <returns>The layout manager.</returns>
        private static DockLayoutManager<CustomToolViewModel> BuildLayoutRoot()
        {
            // Create the layout manager with 3 top level splits.
            DockLayoutManager<CustomToolViewModel> layout = new("left", "center", "right");

            // Add an item to the left panel
            _ = layout.CreateOrUpdateItem("1", "Left Item", new TextBlock { Text = "Left" }, "left");

            // Add an item to the center panel and prevent it from being hidden or closed.
            DockItemViewModel? cantCloseTool = layout.CreateOrUpdateItem("2", "Center Item", new TextBlock { Text = "Center" }, "center");
            cantCloseTool!.DisableClose = true;
            cantCloseTool!.DisableHide = true;
            cantCloseTool.Title = "Center Tool";

            // Add two items to the right tab panel and stack them
            _ = layout.CreateOrUpdateItem("3", "Right Item 1", new TextBlock { Text = "Right 1" }, "right");
            _ = layout.CreateOrUpdateItem("4", "Right Item 2", new TextBlock { Text = "Right 2" }, "right");

            return layout;
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

            this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Monitoring layout changes...";
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

            this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Items collection changed: {action} {eventArgs.NewItems?.Count ?? eventArgs.OldItems?.Count} item(s)";
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

            this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Minimized items collection changed: {action} {eventArgs.NewItems?.Count ?? eventArgs.OldItems?.Count} item(s)";
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
                this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Dock tree structure changed";
                // Here you could also re-subscribe to events on the new dock tree if needed
            }
        }

        /// <summary>
        /// Adds a new item to demonstrate layout changes.
        /// </summary>
        [RelayCommand]
        private void AddItem()
        {
            CustomToolViewModel? newItem = this.LayoutManager.CreateOrUpdateItem(
                $"item_{DateTime.Now.Ticks}",
                "New Item",
                new TextBlock { Text = "Newly added item" },
                "right");

            this.StatusMessage = $"Added new item: {newItem?.Title}";
        }

        /// <summary>
        /// Load the saved layout.
        /// </summary>
        [RelayCommand]
        private void LoadLayout()
        {
            _ = this.LayoutManager.LoadLayout("layout.json");
            this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Layout loaded";
        }

        /// <summary>
        /// Removes an item to demonstrate layout changes.
        /// </summary>
        [RelayCommand]
        private void RemoveItem()
        {
            DockWorkspaceManager workspace = this.LayoutManager.DockControl.PrimaryWorkspace;
            if (workspace != null && workspace.Items.Count > 0)
            {
                DockItemViewModel firstItem = workspace.Items[0];
                firstItem.CloseCommand.Execute(null);
                this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Removed item: {firstItem.Title}";
            }
        }

        /// <summary>
        /// Save the layout.
        /// </summary>
        [RelayCommand]
        private void SaveLayout()
        {
            this.LayoutManager.SaveLayout("layout.json");
            this.StatusMessage = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture)}] Layout saved";
        }
    }
}
