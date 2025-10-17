// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace HelloLayout.ViewModels
{
    /// <summary>
    /// The main view model that controls which view is currently displayed in the application.
    /// </summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the <see cref="DockItemViewModel"/> currently selected in the combo box.
        /// </summary>
        [ObservableProperty]
        private DockItemViewModel? selectedItem;

        /// <summary>
        /// Gets or sets the <see cref="DockInsertPolicy"/> to be used.
        /// </summary>
        [ObservableProperty]
        private DockInsertPolicy insertPolicy = DockInsertPolicy.CreateLast;

        /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
        public MainWindowViewModel()
        {
            this.LayoutManager = BuildLayoutRoot();
        }

        /// <summary>Gets the thing.</summary>
        public DockLayoutManager<CustomToolViewModel> LayoutManager { get; }

        /// <summary>Build a DockLayoutRootViewModel.</summary>.
        /// <returns>The thing built.</returns>
        private static DockLayoutManager<CustomToolViewModel> BuildLayoutRoot()
        {
            // Create the layout manager with 3 top level splits.
            DockLayoutManager<CustomToolViewModel> layout = new("left", "center", "right");

            // Add a item to the left panel
            _ = layout.CreateOrUpdateItem("1", "Left Item", new TextBlock { Text = "Left" }, "left");

            // Add an item to the center panel and prevent it from being hidden or closed.
            DockItemViewModel? cantCloseTool = layout.CreateOrUpdateItem("2", "Center Item", new TextBlock { Text = "Center" }, "center");
            cantCloseTool!.DisableClose = true;
            cantCloseTool!.DisableHide = true;
            cantCloseTool.Title = "Center Tool";

            // Add two item to the right tab panel and stack them
            _ = layout.CreateOrUpdateItem("3", "Right Item 1", new TextBlock { Text = "Right 1" }, "right");
            _ = layout.CreateOrUpdateItem("4", "Right Item 2", new TextBlock { Text = "Right 2" }, "right");

            // Add minimized item and give it affinity to restore to the right tab panel
            CustomToolViewModel? minimizedTool = layout.CreateOrUpdateItem("5", "Minimized Item", new TextBlock { Text = "Minimized" }, "right");
            minimizedTool!.MinimizeCommand.Execute(null);

            return layout;
        }

        /// <summary>Load the saved layout.</summary>
        [RelayCommand]
        private void LoadLayout()
        {
            _ = this.LayoutManager.LoadLayout("layout.json");
        }

        /// <summary>Re-opens a previously closed window..</summary>
        [RelayCommand]
        private void ReopenItem()
        {
            if (this.SelectedItem?.ShowCommand.CanExecute(null) is true)
            {
                this.SelectedItem?.ShowCommand.Execute(null);
            }
        }

        /// <summary>Save the layout.</summary>
        [RelayCommand]
        private void SaveLayout()
        {
            this.LayoutManager.SaveLayout("layout.json");
        }
    }
}
