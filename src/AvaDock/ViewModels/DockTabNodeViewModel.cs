// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.ViewModels
{
    /// <summary>
    /// Defines the view model for use with <see cref="DockTabPanel"/>.
    /// </summary>
    public partial class DockTabNodeViewModel : DockNodeViewModel
    {
        /// <summary>
        /// Gets the currently selected <see cref="DockItemViewModel"/>.
        /// </summary>
        [ObservableProperty]
        private DockItemViewModel? selected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockTabNodeViewModel"/> class.
        /// </summary>
        public DockTabNodeViewModel()
        {
            this.ObservableTabs.CollectionChanged += this.OnTabsItemsChanged;
        }

        /// <summary>Gets list of <see cref="DockItemViewModel"/>s presented as tabs.</summary>
        public IReadOnlyList<DockItemViewModel> Tabs => this.ObservableTabs;

        /// <summary>Gets the mutable collection of <see cref="DockItemViewModel"/>s currently in the node.</summary>
        // TODO: Task 176 to replace this with an event.
        internal ObservableCollection<DockItemViewModel> ObservableTabs { get; } = [];

        /// <summary>Adds a <see cref="DockItemViewModel"/> as a tab.</summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to be added.</param>
        public void AddTab(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            this.ObservableTabs.Add(item);
        }

        /// <summary>Removes all <see cref="DockItemViewModel"/> assigned at tabs.</summary>
        public void ClearTabs()
        {
            this.ObservableTabs.Clear();
        }

        /// <summary>Removes a <see cref="DockItemViewModel"/> that is currently a tab.</summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to be removed.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public Boolean RemoveTab(DockItemViewModel item)
        {
            Int32 index = this.ObservableTabs.IndexOf(item);
            return index < 0 || this.ObservableTabs.Remove(item);
        }

        /// <summary>
        /// Called when the <see cref="Tabs"/> collection is added to or removed from.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="NotifyCollectionChangedEventArgs"/> for the event.</param>
        private void OnTabsItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            IEnumerable<DockItemViewModel> oldMinimizedRemoved = eventArgs.OldItems?.OfType<DockItemViewModel>() ?? [];
            Boolean visibleChanged = oldMinimizedRemoved.Any() || (eventArgs.NewItems?.OfType<DockItemViewModel>().Any() ?? false);

            if (visibleChanged)
            {
                this.OnPropertyChanged(nameof(this.Tabs));
            }
        }
    }
}
