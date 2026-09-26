// Copyright (C) Scott Kupec. All rights reserved.

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
        /// Gets or sets the <see cref="DockItemViewModel"/> currently selected in the combo box for showing
        /// previously hidden items.
        /// </summary>
        [ObservableProperty]
        private DockItemViewModel? selectedItem;

        /// <summary>
        /// Gets or sets the <see cref="DockInsertPolicy"/> to be used.
        /// </summary>
        [ObservableProperty]
        private DockInsertPolicy insertPolicy = DockInsertPolicy.CreateFirst;

        /// <summary>Initializes a new instance of the <see cref="MainWindowViewModel"/> class.</summary>
        public MainWindowViewModel()
        {
            this.LoadLayout();
        }

        /// <summary>Gets the <see cref="DockLayoutManager"/> managing the dock control instance.</summary>
        public DockLayoutManager<CustomToolViewModel> LayoutManager { get; } = new();

        /// <summary>Load the saved layout.</summary>
        [RelayCommand]
        private void LoadLayout()
        {
            // This sample just uses the layout.json that is part of the build. In a production app, we'd
            // also look for a user-specific layout file.
            // TODO: We need to wire up some sort of context factory or context deserialization to make
            //       this complete.
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
