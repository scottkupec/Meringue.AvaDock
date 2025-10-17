// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Events;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Defines the view model for use with <see cref="DockControl"/>.
    /// </summary>
    public partial class DockWorkspaceManager : ObservableObject
    {
        /// <summary>Mutatable backing field for <see cref="Items"/>.</summary>
        private readonly ObservableCollection<DockItemViewModel> items = [];

        /// <summary>Mutatable backing field for <see cref="MinimizedItems"/>.</summary>
        private readonly ObservableCollection<DockItemViewModel> minimizedItems = [];

        /// <summary>
        /// Gets or sets the top-level <see cref="DockSplitNodeViewModel"/>.
        /// </summary>
        [ObservableProperty]
        private DockSplitNodeViewModel dockTree;

        /// <summary>
        /// Gets or sets the <see cref="DockItemViewModel"/> currently hovered over.
        /// </summary>
        /// <remarks>
        /// Used to display minimized item previews when hovering over the minimized tab.
        /// </remarks>
        [ObservableProperty]
        private DockItemViewModel? hoveredItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockWorkspaceManager"/> class.
        /// </summary>
        /// <param name="rootNode">The root <see cref="DockSplitNodeViewModel"/> for the dock control tree.</param>
        public DockWorkspaceManager(DockSplitNodeViewModel rootNode)
        {
            this.DockMonitor = new(this.HookItem, this.UnhookItem);
            this.DockMonitor.Monitor(rootNode);
            this.DockTree = rootNode;
            this.Items = new ReadOnlyObservableCollection<DockItemViewModel>(this.items);
            this.MinimizedItems = new ReadOnlyObservableCollection<DockItemViewModel>(this.minimizedItems);
            this.minimizedItems.CollectionChanged += this.OnMinimizedItemsChanged;
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="Items"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="Items"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// internal backing collection.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? ItemsChanged
        {
            add => this.items.CollectionChanged += value;
            remove => this.items.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="MinimizedItems"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="MinimizedItems"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// internal backing collection.
        /// </remarks>
        // Intentionally not tied directly to this.minimizedItems so we can defer events
        // until after we've run this.DockTree.RemoveEmptyPanels when necessary and not
        // have race conditions for the state of the tree.
        public event NotifyCollectionChangedEventHandler? MinimizedItemsChanged;

        /// <summary>
        /// Gets the id of the current instance.
        /// </summary>
        public String Id { get; init; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently soft-closed and not part of the visuals.
        /// </summary>
        public ReadOnlyObservableCollection<DockItemViewModel> Items { get; }

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently minimized and represented by <see cref="MinimizedItemsBar"/>s.
        /// </summary>
        public ReadOnlyObservableCollection<DockItemViewModel> MinimizedItems { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="MinimizedItems"/> strip should be displayed.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Used to enable the UI display of the minimized tabs area.
        public Boolean ShouldShowMinimizedItems => this.MinimizedItems.Count > 0;

        /// <summary>
        /// Gets a property dictionary usable for implementation specific purposes.
        /// </summary>
        public Dictionary<Object, Object> Tags { get; } = [];

        /// <summary>
        /// Gets the <see cref="DockNodeMonitor"/> used to monitor for changes in <see cref="DockTree"/>s.
        /// </summary>
        private DockNodeMonitor DockMonitor { get; }

        /// <summary>
        /// Adds a <see cref="DockItemViewModel"/> to the current instance.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        /// <returns><c>true</c> if the <see cref="DockItemViewModel"/> is part of the current instance; otherwise <c>false</c>.</returns>
        public Boolean AddItem(DockItemViewModel item)
        {
            Boolean success = false;

            if (!this.minimizedItems.Contains(item))
            {
                DockTabNodeViewModel? tabNode = null;

                // Prefer to insert back to the original node.
                String? preferredTabNode = DockContext.GetPreferredTabPanelId(item);

                if (preferredTabNode is not null)
                {
                    tabNode = this.DockTree.FindNode(preferredTabNode) as DockTabNodeViewModel;
                }

                // Fallback to the first available tab node.
                tabNode ??= this.DockTree.FindFirstTabNode();

                if (tabNode is null)
                {
                    tabNode = preferredTabNode is not null
                        ? new DockTabNodeViewModel() { Id = preferredTabNode }
                        : new DockTabNodeViewModel();

                    if (this.DockTree is DockSplitNodeViewModel split)
                    {
                        split.AddChild(tabNode);
                    }
                    else
                    {
                        throw new InvalidOperationException($"{nameof(this.DockTree)} must be a {nameof(DockTabNodeViewModel)} or a {nameof(DockSplitNodeViewModel)}.");
                    }
                }

                tabNode.AddTab(item);

                DockContext.ClearPreferredTabPanelId(item);
                success = true;
            }

            return success;
        }

        /// <summary>
        /// Signals that a batch of structural changes to the workspace is complete.
        /// </summary>
        /// <param name="collapseTree">
        /// If <c>true</c>, empty nodes will be collapsed from the tree; otherwise, empty nodes will not be removed.
        /// </param>
        /// <remarks>
        /// This method should be called by layout managers or mutation orchestrators after completing a series of
        /// insertions, removals, or replacements to ensure the UI reflects the updated logical structure.
        /// It avoids premature or repeated layout rebuilds during intermediate mutation steps.
        /// </remarks>
        public void CommitChanges(Boolean collapseTree = true)
        {
            if (collapseTree)
            {
                this.DockTree.RemoveEmptyPanels();
            }

            this.DockTree.CommitChanges();
        }

        /// <summary>Find an existing <see cref="DockItemViewModel"/> by id.</summary>
        /// <param name="itemId">The id of the <see cref="DockItemViewModel"/> to find.</param>
        /// <returns>The <see cref="DockItemViewModel"/> found or <c>null</c> if no such <see cref="DockItemViewModel"/> exists.</returns>
        public DockItemViewModel? FindItem(String itemId)
        {
            if (String.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            DockItemViewModel? existingItem = this.DockTree.FindItem<DockItemViewModel>(itemId);
            existingItem ??= this.MinimizedItems.FirstOrDefault(item => item.Id == itemId);
            return existingItem;
        }

        /// <summary>
        /// Removes a <see cref="DockItemViewModel"/> from the current instance.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        /// <returns><c>true</c> if the <see cref="DockItemViewModel"/> is no longer part of the current instance; otherwise <c>false</c>.</returns>
        public Boolean RemoveItem(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            Boolean success = false;

            if (!this.minimizedItems.Contains(item))
            {
                DockTabNodeViewModel? tabNode = this.DockTree.FindOwningTabNode(item.Id);

                if (tabNode is not null)
                {
                    if (tabNode.Tabs.Count == 1)
                    {
                        DockSplitNodeViewModel? split = this.DockTree.GetContainingSplit(tabNode);
                        System.Diagnostics.Debug.Assert(split is not null, "Unable to find containing split node.");
                        split?.RemoveChild(tabNode);
                        success = true;
                    }
                    else if (tabNode.RemoveTab(item))
                    {
                        success = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false, "Failed to remove item from workspace.");
                    }
                }
                else
                {
                    success = true;
                }
            }
            else
            {
                success = this.minimizedItems.Remove(item);
                this.MinimizedItemsChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }

            return success;
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current instance
        /// will be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void HookItem(DockItemViewModel item)
        {
            item.HideRequested += this.OnItemHideRequested;
            item.MinimizeRequested += this.OnItemMinimizeRequested;
            item.RestoreRequested += this.OnItemRestoreRequested;
            DockContext.SetWorkspace(item, this);
            this.items.Add(item);
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be hidden in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be hidden.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the hide request.</param>
        private void OnItemHideRequested(Object? sender, DockItemHideRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;
            DockTabNodeViewModel? owningTab = this.DockTree.FindOwningTabNode(item.Id);

            if (owningTab is not null)
            {
                DockContext.SetPreferredTabPanelId(item, owningTab.Id);
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be minimized in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be minimized.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the minize request.</param>
        private void OnItemMinimizeRequested(Object? sender, DockItemMinimizeRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;
            System.Diagnostics.Debug.Assert(!this.minimizedItems.Contains(item), "Item should not already be minimized when minimizing it.");

            DockTabNodeViewModel? owningTab = this.DockTree.FindOwningTabNode(item.Id);

            if (owningTab is not null)
            {
                DockContext.SetPreferredTabPanelId(item, owningTab.Id);
                Boolean removed = this.RemoveItem(item);
                System.Diagnostics.Debug.Assert(removed, "Failed to remove item from DockTree. Not minimized.");

                if (removed)
                {
                    this.minimizedItems.Add(item);
                    this.CommitChanges();

                    this.MinimizedItemsChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be restored from the minimized state
        /// back to its original <see cref="DockTabNodeViewModel"/> panel.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested restoration.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the restore request.</param>
        private void OnItemRestoreRequested(Object? sender, DockItemRestoreRequestedEventArgs eventArgs)
        {
            this.HoveredItem = null;

            DockItemViewModel item = eventArgs.Item;
            Boolean success = this.minimizedItems.Remove(item);

            if (success)
            {
                _ = this.AddItem(item);
                this.CommitChanges();

                this.MinimizedItemsChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
        }

        /// <summary>
        /// Handles changes to the <see cref="MinimizedItems"/> collection by attaching
        /// or detaching event handlers for each added or removed <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Object"/> that raised the event (the <see cref="ObservableCollection{T}"/>).
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> containing details
        /// about which items were added, removed, or reset.
        /// </param>
        private void OnMinimizedItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action is NotifyCollectionChangedAction.Add && eventArgs.NewItems is not null)
            {
                foreach (DockItemViewModel item in eventArgs.NewItems)
                {
                    this.HookItem(item);
                }
            }

            if (eventArgs.Action is NotifyCollectionChangedAction.Remove && eventArgs.OldItems is not null)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems)
                {
                    this.UnhookItem(item);
                }
            }

            this.OnPropertyChanged(nameof(this.ShouldShowMinimizedItems));
        }

        /// <summary>
        /// Removes all handlers for a <see cref="DockItemViewModel"/> so the current instance
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void UnhookItem(DockItemViewModel item)
        {
            item.HideRequested -= this.OnItemHideRequested;
            item.MinimizeRequested -= this.OnItemMinimizeRequested;
            item.RestoreRequested -= this.OnItemRestoreRequested;
            DockContext.ClearWorkspace(item);
            _ = this.items.Remove(item);
        }

        /// <inheritdoc/>
        partial void OnDockTreeChanged(DockSplitNodeViewModel? oldValue, DockSplitNodeViewModel newValue)
        {
            if (newValue is not null)
            {
                this.DockMonitor.Monitor(newValue);
            }
        }

        /// <inheritdoc/>
        partial void OnDockTreeChanging(DockSplitNodeViewModel? oldValue, DockSplitNodeViewModel newValue)
        {
            if (oldValue is not null)
            {
                this.DockMonitor.Unmonitor(oldValue);
            }
        }
    }
}
