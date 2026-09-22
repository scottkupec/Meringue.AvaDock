// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Layout;
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
        /// <summary>Mutatable backing field for <see cref="HiddenItems"/>.</summary>
        private readonly ObservableCollection<DockItemViewModel> hiddenItems = [];

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
            this.DockMonitor = new(this.HookItem, this.UnhookItem, this.HookNode, this.UnhookNode);
            this.DockMonitor.Monitor(rootNode);
            this.DockTree = rootNode;
            this.hiddenItems.CollectionChanged += this.OnHiddenItemsChanged;
            this.Items = new ReadOnlyObservableCollection<DockItemViewModel>(this.items);
            this.MinimizedItems = new ReadOnlyObservableCollection<DockItemViewModel>(this.minimizedItems);
            this.minimizedItems.CollectionChanged += this.OnMinimizedItemsChanged;
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="HiddenItems"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="HiddenItems"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// private backing collection.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? HiddenItemsChanged
        {
            add => this.hiddenItems.CollectionChanged += value;
            remove => this.hiddenItems.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="Items"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="Items"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// private backing collection.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? ItemsChanged
        {
            add => this.items.CollectionChanged += value;
            remove => this.items.CollectionChanged -= value;
        }

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been closed.
        /// </summary>
        public event EventHandler<DockItemClosedEventArgs>? ItemClosed;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being closed
        /// </summary>
        public event EventHandler<DockItemClosingEventArgs>? ItemClosing;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been hidden.
        /// </summary>
        public event EventHandler<DockItemHiddenEventArgs>? ItemHidden;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being hidden
        /// </summary>
        public event EventHandler<DockItemHidingEventArgs>? ItemHiding;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been minimized.
        /// </summary>
        public event EventHandler<DockItemMinimizedEventArgs>? ItemMinimized;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being minimized.
        /// </summary>
        public event EventHandler<DockItemMinimizingEventArgs>? ItemMinimizing;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been moved.
        /// </summary>
        public event EventHandler<DockItemMovedEventArgs>? ItemMoved;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being moved.
        /// </summary>
        public event EventHandler<DockItemMovingEventArgs>? ItemMoving;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been restored from a minimized state.
        /// </summary>
        public event EventHandler<DockItemRestoredEventArgs>? ItemRestored;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being restored from a minimized state.
        /// </summary>
        public event EventHandler<DockItemRestoringEventArgs>? ItemRestoring;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> has been shown (restored from hidden state)..
        /// </summary>
        public event EventHandler<DockItemShownEventArgs>? ItemShown;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being shown (restored from hidden state).
        /// </summary>
        public event EventHandler<DockItemShowingEventArgs>? ItemShowing;

        /// <summary>
        /// Occurs when the contents of the <see cref="MinimizedItems"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="MinimizedItems"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// private backing collection.
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
        public IEnumerable<DockItemViewModel> HiddenItems => this.hiddenItems;

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

            if (this.minimizedItems.Contains(item))
            {
                return false;
            }

            if (this.hiddenItems.Contains(item))
            {
                return false;
            }

            if (this.items.Contains(item))
            {
                return false;
            }

            DockItemState? state = DockContext.GetItemState(item);

            if (state is DockItemState.Minimized)
            {
                this.minimizedItems.Add(item);
            }
            else if (state is DockItemState.Hidden)
            {
                this.hiddenItems.Add(item);
            }
            else
            {
                if (state is null)
                {
                    DockContext.SetItemState(item, DockItemState.Normal);
                }

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
            existingItem ??= this.HiddenItems.FirstOrDefault(item => item.Id == itemId);
            return existingItem;
        }

        /// <summary>
        /// Minimizes a <see cref="DockItemViewModel"/> from the current instance.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        /// <returns><c>true</c> if the <see cref="DockItemViewModel"/> was minimized; otherwise <c>false</c>.</returns>
        public Boolean MinimizeItem(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            Boolean success = false;

            if (!this.minimizedItems.Contains(item))
            {
                DockTabNodeViewModel? tabNode = this.DockTree.FindOwningTabNode(item.Id);

                if (tabNode is not null)
                {
                    DockContext.SetPreferredTabPanelId(item, tabNode.Id);

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

                if (success)
                {
                    this.CommitChanges();
                    this.minimizedItems.Add(item);
                    this.MinimizedItemsChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
            }

            return success;
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

            if (this.minimizedItems.Contains(item))
            {
                success = this.minimizedItems.Remove(item);

                if (success)
                {
                    this.MinimizedItemsChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }
            else if (this.hiddenItems.Contains(item))
            {
                success = this.hiddenItems.Remove(item);
            }
            else
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

            return success;
        }

        /// <summary>
        /// Restores a <see cref="DockItemViewModel"/> from the current instance.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        /// <returns><c>true</c> if the <see cref="DockItemViewModel"/> was restored; otherwise <c>false</c>.</returns>
        public Boolean RestoreItem(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            Boolean success = false;

            if (this.minimizedItems.Contains(item))
            {
                if (this.minimizedItems.Remove(item))
                {
                    DockContext.SetItemState(item, DockItemState.Normal);
                    _ = this.AddItem(item);
                    this.CommitChanges();
                    this.MinimizedItemsChanged?.Invoke(
                        this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
            }

            return success;
        }

        /// <summary>
        /// Raises the <see cref="ItemClosed"/> event when an item is closed.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemClosedEventArgs"/> for the event.</param>
        protected virtual void OnItemClosed(DockItemClosedEventArgs eventArgs) => this.ItemClosed?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemClosing"/> event when item is in the process of being closed.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemClosingEventArgs"/> for the event.</param>
        protected virtual void OnItemClosing(DockItemClosingEventArgs eventArgs) => this.ItemClosing?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemHidden"/> event when an item is hidden.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemHiddenEventArgs"/> for the event.</param>
        protected virtual void OnItemHidden(DockItemHiddenEventArgs eventArgs) => this.ItemHidden?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemHiding"/> event when a item is being hidden.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemHidingEventArgs"/> for the event.</param>
        protected virtual void OnItemHiding(DockItemHidingEventArgs eventArgs) => this.ItemHiding?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemMinimized"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemMinimizedEventArgs"/> that is being attached.</param>
        protected virtual void OnItemMinimized(DockItemMinimizedEventArgs eventArgs) => this.ItemMinimized?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemMinimizing"/> event when an item is in the process of being minimized.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemMinimizingEventArgs"/> that is being attached.</param>
        protected virtual void OnItemMinimizing(DockItemMinimizingEventArgs eventArgs) => this.ItemMinimizing?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemMoved"/> event when an item has been moved.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemMovedEventArgs"/> for the event.</param>
        protected virtual void OnItemMoved(DockItemMovedEventArgs eventArgs) => this.ItemMoved?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemMoving"/> event when an item is in the process of being moved.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemMovingEventArgs"/> for the event.</param>
        protected virtual void OnItemMoving(DockItemMovingEventArgs eventArgs) => this.ItemMoving?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemRestored"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemRestoredEventArgs"/> for the event.</param>
        protected virtual void OnItemRestored(DockItemRestoredEventArgs eventArgs) => this.ItemRestored?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemRestoring"/> event when an item is in the process of being restored.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemRestoringEventArgs"/> for the event.</param>
        protected virtual void OnItemRestoring(DockItemRestoringEventArgs eventArgs) => this.ItemRestoring?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemShown"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemShownEventArgs"/> for the event.</param>
        protected virtual void OnItemShown(DockItemShownEventArgs eventArgs) => this.ItemShown?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="ItemShowing"/> event when a workspace is in the process of being attached.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockItemShowingEventArgs"/> for the event.</param>
        protected virtual void OnItemShowing(DockItemShowingEventArgs eventArgs) => this.ItemShowing?.Invoke(this, eventArgs);

        /// <summary>
        /// Ensures that the workspace always has at least one <see cref="DockTabNodeViewModel"/>
        /// so that it is always a valid drop target.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to validate.</param>
        private static void EnsureWorkspaceHasTabNode(DockWorkspaceManager workspace)
        {
            if (workspace.DockTree is not DockSplitNodeViewModel splitNode)
            {
                return;
            }

            if (!splitNode.Children.Any())
            {
                splitNode.AddChild(new DockTabNodeViewModel() { Id = $"default:{Guid.NewGuid()}" });
                workspace.CommitChanges(collapseTree: false);
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be closed.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested closing.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the close request.</param>
        private void HandleItemCloseRequested(Object? sender, DockItemCloseRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;
            DockItemClosingEventArgs closingEventArgs = new(item);
            this.OnItemClosing(closingEventArgs);

            if (!closingEventArgs.Cancel)
            {
                if (this.hiddenItems.Contains(item))
                {
                    _ = this.hiddenItems.Remove(item);
                }
                else if (this.minimizedItems.Contains(item))
                {
                    _ = this.minimizedItems.Remove(item);
                }
                else
                {
                    DockTabNodeViewModel? tabNode = this.DockTree.FindOwningTabNode(item.Id);
                    System.Diagnostics.Debug.Assert(tabNode is not null, "Couldn't find item being closed.");

                    if (tabNode is not null)
                    {
                        _ = tabNode.RemoveTab(item);
                        this.CommitChanges();
                    }
                }

                this.OnItemClosed(new DockItemClosedEventArgs(item));
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be hidden in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be hidden.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the hide request.</param>
        private void HandleItemHideRequested(Object? sender, DockItemHideRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;

            if (!this.hiddenItems.Contains(item))
            {
                if (this.Items.Contains(item))
                {
                    DockItemHidingEventArgs hidingEventArgs = new(item);
                    this.OnItemHiding(hidingEventArgs);

                    if (!hidingEventArgs.Cancel)
                    {
                        DockContext.SetPreferredWorkspaceId(item, this.Id);

                        if (this.RemoveItem(item))
                        {
                            DockContext.SetItemState(item, DockItemState.Hidden);
                            this.hiddenItems.Add(item);
                            this.OnPropertyChanged(nameof(this.HiddenItems));
                            this.CommitChanges();
                            this.OnItemHidden(new DockItemHiddenEventArgs(item));
                        }
                    }
                }

                DockWorkspaceManager.EnsureWorkspaceHasTabNode(this);
            }
            else
            {
                System.Diagnostics.Debug.Assert(!this.hiddenItems.Contains(item), "Can't re-hide a hidden item.");
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be minimized in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be minimized.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the minize request.</param>
        private void HandleItemMinimizeRequested(Object? sender, DockItemMinimizeRequestedEventArgs eventArgs)
        {
            this.HoveredItem = null;
            DockItemViewModel item = eventArgs.Item;
            System.Diagnostics.Debug.Assert(!this.MinimizedItems.Contains(item), "Item should not already be minimized when minimizing it.");

            DockTabNodeViewModel? owningTab = this.DockTree.FindOwningTabNode(item.Id);

            if (owningTab is not null)
            {
                DockItemMinimizingEventArgs minimizingEventArgs = new(item);
                this.OnItemMinimizing(minimizingEventArgs);

                if (!minimizingEventArgs.Cancel)
                {
                    if (this.MinimizeItem(item))
                    {
                        DockContext.SetItemState(item, DockItemState.Minimized);
                        this.OnItemMinimized(new DockItemMinimizedEventArgs(item));
                    }
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be moved in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested the move.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the move request.</param>
        private void HandleItemMoveRequested(Object? sender, DockItemMoveRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;
            DockTabNodeViewModel targetTabNode = eventArgs.ToNode;
            DockTabNodeViewModel? sourceTabNode = eventArgs.FromNode ?? this.DockTree.FindOwningTabNode(item.Id);
            MovePlacement placement = eventArgs.Placement;
            Orientation? requiredOrientation = eventArgs.RequiredOrientation;

            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            TargetFrameworkHelper.ThrowIfArgumentNull(targetTabNode);
            TargetFrameworkHelper.ThrowIfArgumentNull(sourceTabNode);

            DockItemMovingEventArgs movingEventArgs = new(item, sourceTabNode, targetTabNode, placement, requiredOrientation);

            if (!movingEventArgs.Cancel)
            {
                MoveOperation operation = new(
                    this,
                    item,
                    targetTabNode,
                    placement,
                    requiredOrientation);

                Boolean result = operation.Execute();
                DockWorkspaceManager.EnsureWorkspaceHasTabNode(this);

                if (result)
                {
                    this.OnItemMoved(new DockItemMovedEventArgs(item, sourceTabNode, targetTabNode, placement, requiredOrientation));
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be restored from the minimized state
        /// back to its original <see cref="DockTabNodeViewModel"/> panel.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested restoration.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the restore request.</param>
        private void HandleItemRestoreRequested(Object? sender, DockItemRestoreRequestedEventArgs eventArgs)
        {
            this.HoveredItem = null;

            DockItemViewModel item = eventArgs.Item;
            DockItemRestoringEventArgs restoringEventArgs = new(item);

            this.OnItemRestoring(restoringEventArgs);

            if (!restoringEventArgs.Cancel)
            {
                if (this.RestoreItem(item))
                {
                    DockContext.SetItemState(item, DockItemState.Normal);
                    this.OnItemRestored(new DockItemRestoredEventArgs(item));
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be show in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be shown.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the show request.</param>
        private void HandleItemShowRequested(Object? sender, DockItemShowRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;

            if (this.hiddenItems.Contains(item))
            {
                DockItemShowingEventArgs showingEventArgs = new(item);
                this.OnItemShowing(showingEventArgs);

                if (!showingEventArgs.Cancel)
                {
                    if (this.hiddenItems.Remove(item))
                    {
                        DockContext.SetItemState(item, DockItemState.Normal);
                        Boolean added = this.AddItem(item);
                        System.Diagnostics.Debug.Assert(added, "Failed to add item to workspace during show request.");

                        this.OnPropertyChanged(nameof(this.HiddenItems));
                        DockContext.ClearPreferredWorkspaceId(item);
                        this.CommitChanges();
                        DockContext.SetItemState(item, DockItemState.Normal);
                        this.OnItemShown(new DockItemShownEventArgs(item));
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(this.hiddenItems.Contains(item), "Can't show an item that isn't hidden.");
            }
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current instance
        /// will be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void HookItem(DockItemViewModel item)
        {
            this.items.Add(item);
            this.MonitorItem(item);
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockNodeViewModel"/> so the current instance
        /// will be notified of changes.
        /// </summary>
        /// <param name="node">The <see cref="DockNodeViewModel"/> to process.</param>
        private void HookNode(DockNodeViewModel node)
        {
            if (node is DockTabNodeViewModel)
            {
                DockContext.SetWorkspace(node, this);
            }
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current instance
        /// will be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void MonitorItem(DockItemViewModel item)
        {
            this.UnmonitorItem(item); // Safety mechanism for HiddenItems and MinimizedItems collection changes.
            DockContext.SetWorkspace(item, this);
            item.CloseRequested += this.HandleItemCloseRequested;
            item.HideRequested += this.HandleItemHideRequested;
            item.MinimizeRequested += this.HandleItemMinimizeRequested;
            item.MoveRequested += this.HandleItemMoveRequested;
            item.RestoreRequested += this.HandleItemRestoreRequested;
            item.ShowRequested += this.HandleItemShowRequested;
        }

        /// <summary>
        /// Handles changes to the <see cref="hiddenItems"/> collection by attaching
        /// or detaching event handlers for each added or removed <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Object"/> that raised the event (the <see cref="ObservableCollection{T}"/>).
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> containing details
        /// about which items were added, removed, or reset.
        /// </param>
        private void OnHiddenItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action is NotifyCollectionChangedAction.Add && eventArgs.NewItems is not null)
            {
                foreach (DockItemViewModel item in eventArgs.NewItems)
                {
                    this.MonitorItem(item);
                }
            }

            if (eventArgs.Action is NotifyCollectionChangedAction.Remove && eventArgs.OldItems is not null)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems)
                {
                    this.UnmonitorItem(item);
                }
            }

            // Reset - collection replaced (rare, but possible)
            // Note: For Reset, OldItems/NewItems are null per ObservableCollection contract.
            // We cannot reliably unmonitor items that were removed, so items removed during a Reset
            // may retain their request handlers until they are garbage collected. This is acceptable
            // because the collection is owned by this workspace and items are not externally retained;
            // the handlers are weak references from the item to this manager, not the reverse.
            // MonitorItem is idempotent, so re-monitoring current items is safe.
            if (eventArgs.Action is NotifyCollectionChangedAction.Reset)
            {
                System.Diagnostics.Debug.WriteLine($"[DockWorkspaceManager] HiddenItems Reset detected. OldItems count: {eventArgs.OldItems?.Count ?? 0}");

                foreach (DockItemViewModel item in eventArgs.OldItems ?? Array.Empty<DockItemViewModel>())
                {
                    this.UnmonitorItem(item);
                }

                foreach (DockItemViewModel item in this.hiddenItems)
                {
                    this.MonitorItem(item);
                }
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
                    this.MonitorItem(item);
                }
            }

            if (eventArgs.Action is NotifyCollectionChangedAction.Remove && eventArgs.OldItems is not null)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems)
                {
                    this.UnmonitorItem(item);
                }
            }

            // Reset - collection replaced (rare, but possible)
            // Note: For Reset, OldItems/NewItems are null per ObservableCollection contract.
            // We cannot reliably unmonitor items that were removed, so items removed during a Reset
            // may retain their request handlers until they are garbage collected. This is acceptable
            // because the collection is owned by this workspace and items are not externally retained;
            // the handlers are weak references from the item to this manager, not the reverse.
            // MonitorItem is idempotent, so re-monitoring current items is safe.
            if (eventArgs.Action is NotifyCollectionChangedAction.Reset)
            {
                System.Diagnostics.Debug.WriteLine($"[DockWorkspaceManager] MinimizedItems Reset detected. OldItems count: {eventArgs.OldItems?.Count ?? 0}");

                foreach (DockItemViewModel item in eventArgs.OldItems ?? Array.Empty<DockItemViewModel>())
                {
                    this.UnmonitorItem(item);
                }

                foreach (DockItemViewModel item in this.minimizedItems)
                {
                    this.MonitorItem(item);
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
            _ = this.items.Remove(item);
            this.UnmonitorItem(item);
        }

        /// <summary>
        /// Removes all handlers for a <see cref="DockNodeViewModel"/> so the current instance
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="node">The <see cref="DockNodeViewModel"/> to process.</param>
        private void UnhookNode(DockNodeViewModel node)
        {
            if (node is DockTabNodeViewModel)
            {
                DockContext.ClearWorkspace(node);
            }
        }

        /// <summary>
        /// Removes all handlers for a <see cref="DockItemViewModel"/> so the current instance
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void UnmonitorItem(DockItemViewModel item)
        {
            item.CloseRequested -= this.HandleItemCloseRequested;
            item.HideRequested -= this.HandleItemHideRequested;
            item.MinimizeRequested -= this.HandleItemMinimizeRequested;
            item.MoveRequested -= this.HandleItemMoveRequested;
            item.RestoreRequested -= this.HandleItemRestoreRequested;
            item.ShowRequested -= this.HandleItemShowRequested;
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

        /// <summary>
        /// Encapsulates the logic for moving a <see cref="DockItemViewModel"/> within the dock layout.
        /// </summary>
        private sealed class MoveOperation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MoveOperation"/> class.
            /// </summary>
            /// <param name="owner">The <see cref="DockWorkspaceManager"/> that owns this operation.</param>
            /// <param name="item">The <see cref="DockItemViewModel"/> being moved.</param>
            /// <param name="targetNode">The target node to which the <see cref="DockItemViewModel"/> is being moved.</param>
            /// <param name="placement">How the item should be placed relative to the target node.</param>
            /// <param name="orientation">The orientation required for the placing the <see cref="DockItemViewModel"/>.</param>
            public MoveOperation(
                DockWorkspaceManager owner,
                DockItemViewModel item,
                DockTabNodeViewModel targetNode,
                MovePlacement placement,
                Orientation? orientation)
            {
                this.Item = item;
                this.Owner = owner;
                this.Placement = placement;
                this.RequiredOrientation = orientation;
                this.TargetNode = targetNode;
            }

            /// <summary>
            /// Gets the <see cref="MovePlacement"/> for how the item should be placed in <see cref="TargetNode"/>.
            /// </summary>
            public MovePlacement Placement { get; }

            /// <summary>
            /// Gets the <see cref="MovePlacement"/> for how the item should be placed in <see cref="TargetNode"/> when <see cref="Placement"/>
            /// is <see cref="MovePlacement.After"/> or <see cref="MovePlacement.Before"/>.
            /// </summary>
            public Orientation? RequiredOrientation { get; }

            /// <summary>
            /// Gets the <see cref="DockWorkspaceManager"/> that owns this operation.
            /// </summary>
            private DockWorkspaceManager Owner { get; }

            /// <summary>
            /// Gets the <see cref="DockNodeViewModel"/> to which the <see cref="DockItemViewModel"/> is being moved.
            /// </summary>
            private DockNodeViewModel TargetNode { get; }

            /// <summary>
            /// Gets the <see cref="DockItemViewModel"/> being moved.
            /// </summary>
            private DockItemViewModel Item { get; }

            /// <summary>
            /// Executes the move operation.
            /// </summary>
            /// <returns><c>true</c> if the <see cref="DockItemViewModel"/> was successfully moved; otherwise, <c>false</c>.</returns>
            public Boolean Execute()
            {
                if (!MoveOperation.IsOperationValid(this))
                {
                    return false;
                }

                Boolean result;
                //// DockTabNodeViewModel sourceTabNode = this.Owner.FindParentTabNode(this.Item)!;
                DockTabNodeViewModel sourceTabNode = this.FindParentTabNode(this.Item)!;

                if (this.Placement == MovePlacement.On && sourceTabNode == this.TargetNode && sourceTabNode.Tabs.Count == 1)
                {
                    result = true; // No-op move
                }
                else
                {
                    result = this.Placement == MovePlacement.On
                        ? MoveOperation.HandleDropCenter(this, sourceTabNode)
                        : MoveOperation.HandleDropSplit(this, sourceTabNode);
                }

                return result;
            }

            /// <summary>
            /// Handles dropping a <see cref="DockItemViewModel"/> to <see cref="DropZone.Center"/> of a <see cref="DockNodeViewModel"/>.
            /// </summary>
            /// <param name="operation">The <see cref="MoveOperation"/> being processed.</param>
            /// <returns>The <see cref="DockSplitNodeViewModel"/> created.</returns>
            private static DockSplitNodeViewModel CreateSplit(MoveOperation operation)
            {
                DockTabNodeViewModel newTabNode = new();
                newTabNode.AddTab(operation.Item);

                Orientation splitOrientation = operation.RequiredOrientation!.Value;

                DockSplitNodeViewModel wrappedSplit = new(splitOrientation);
                if (operation.Placement is MovePlacement.Before)
                {
                    wrappedSplit.AddChild(newTabNode);
                    wrappedSplit.AddChild(operation.TargetNode);
                }
                else
                {
                    wrappedSplit.AddChild(operation.TargetNode);
                    wrappedSplit.AddChild(newTabNode);
                }

                return wrappedSplit;
            }

            /// <summary>
            /// Handles dropping a <see cref="DockItemViewModel"/> to <see cref="DropZone.Center"/> of a <see cref="DockNodeViewModel"/>.
            /// </summary>
            /// <param name="operation">The <see cref="MoveOperation"/> being processed.</param>
            /// <param name="sourceTabNode">The <see cref="DockTabNodeViewModel"/> that contains the <see cref="DockItemViewModel"/> being dropped.</param>
            /// <returns><c>true</c> if the operation was successfully moved; otherwise, <c>false</c>.</returns>
            private static Boolean HandleDropCenter(MoveOperation operation, DockTabNodeViewModel sourceTabNode)
            {
                Boolean result = false;

                System.Diagnostics.Debug.Assert(operation.Placement == MovePlacement.On, "Invalid code path.");

                ////DockWorkspaceManager sourceWorkspace = operation.Owner.GetWorkspace(sourceTabNode)!;
                ////DockWorkspaceManager? destinationWorkspace = operation.Owner.GetWorkspace(operation.TargetNode);

                DockWorkspaceManager? sourceWorkspace = DockContext.GetWorkspace(sourceTabNode); //// operation.Owner.GetWorkspace(sourceTabNode)!;
                DockWorkspaceManager? destinationWorkspace = DockContext.GetWorkspace(operation.TargetNode); //// operation.Owner.GetWorkspace(operation.TargetNode);

                if (operation.TargetNode is DockTabNodeViewModel targetTabNode && sourceWorkspace is not null)
                {
                    if (sourceWorkspace.RemoveItem(operation.Item))
                    {
                        targetTabNode.AddTab(operation.Item);
                        result = true;
                        sourceWorkspace.CommitChanges();

                        if (sourceWorkspace != destinationWorkspace)
                        {
                            destinationWorkspace?.CommitChanges();
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("DropZone.Center requires a DockTabNodeViewModel target.");
                }

                return result;
            }

            /// <summary>
            /// Handles dropping a <see cref="DockItemViewModel"/> to <see cref="DropZone.Center"/> of a <see cref="DockNodeViewModel"/>.
            /// </summary>
            /// <param name="operation">The <see cref="MoveOperation"/> being processed.</param>
            /// <param name="sourceTabNode">The <see cref="DockTabNodeViewModel"/> that contains the <see cref="DockItemViewModel"/> being dropped.</param>
            /// <returns><c>true</c> if the operation was successfully moved; otherwise, <c>false</c>.</returns>
            private static Boolean HandleDropSplit(MoveOperation operation, DockTabNodeViewModel sourceTabNode)
            {
                Boolean result = false;

                DockWorkspaceManager? sourceWorkspace = DockContext.GetWorkspace(sourceTabNode); //// operation.Owner.GetWorkspace(sourceTabNode)!;
                DockWorkspaceManager? destinationWorkspace = DockContext.GetWorkspace(operation.TargetNode); //// operation.Owner.GetWorkspace(operation.TargetNode);
                DockSplitNodeViewModel wrappedSplit = MoveOperation.CreateSplit(operation);
                DockSplitNodeViewModel? parentSplit = destinationWorkspace?.DockTree.GetContainingSplit(operation.TargetNode);

                if (parentSplit is not null && sourceWorkspace is not null && destinationWorkspace is not null)
                {
                    Int32 targetIndex = parentSplit.IndexOf(operation.TargetNode);

                    if (targetIndex >= 0 && sourceWorkspace.RemoveItem(operation.Item))
                    {
                        // Must recalculate targetIndex because the RemoveItem() call may have changed the
                        // targetNode's location.
                        targetIndex = parentSplit.IndexOf(operation.TargetNode);
                        parentSplit.ReplaceChildAt(targetIndex, wrappedSplit);

                        DockSplitNodeViewModel? grandparentSplit = destinationWorkspace?.DockTree.GetContainingSplit(parentSplit);
                        if (grandparentSplit is not null)
                        {
                            Int32 parentIndex = grandparentSplit.IndexOf(parentSplit);
                            if (parentIndex >= 0 && grandparentSplit.Orientation != parentSplit.Orientation)
                            {
                                DockSplitNodeViewModel wrap = new(grandparentSplit.Orientation == Orientation.Horizontal
                                    ? Orientation.Vertical
                                    : Orientation.Horizontal);

                                foreach (DockNodeViewModel child in parentSplit.Children)
                                {
                                    wrap.AddChild(child);
                                }

                                grandparentSplit.ReplaceChildAt(parentIndex, wrap);
                            }
                        }

                        result = true;
                        sourceWorkspace.CommitChanges();

                        if (sourceWorkspace != destinationWorkspace)
                        {
                            // Needed specifically for dropping to the default panel in the PrimaryWorkspace when the
                            // panel is currently empty.
                            destinationWorkspace?.CommitChanges();
                        }
                    }
                }

                return result;
            }

            /// <summary>
            /// Verifies a <see cref="MoveOperation"/> is valid.
            /// </summary>
            /// <param name="operation">The <see cref="MoveOperation"/> to validate.</param>
            /// <returns><c>true</c> if <paramref name="operation"/> is valid; otherwise, <c>false</c>.</returns>
            private static Boolean IsOperationValid(MoveOperation operation)
            {
                DockWorkspaceManager? destinationWorkspace = DockContext.GetWorkspace(operation.TargetNode); //// operation.Owner.GetWorkspace(operation.TargetNode);
                DockTabNodeViewModel? sourceTabNode = operation.Owner.DockTree.FindOwningTabNode(operation.Item.Id);

                if (sourceTabNode is null || destinationWorkspace is null)
                {
                    return false;
                }

                DockWorkspaceManager? workspace = DockContext.GetWorkspace(sourceTabNode);
                return workspace is not null;
            }

            /// <summary>Finds which, if any, owned <see cref="DockTabNodeViewModel"/> contains the given <see cref="DockItemViewModel"/>.</summary>
            /// <param name="item">The <see cref="DockItemViewModel"/> whose owner we want to find.</param>
            /// <returns>The root found, if any.</returns>
            private DockTabNodeViewModel? FindParentTabNode(DockItemViewModel item)
            {
                DockTabNodeViewModel? parent = this.Owner.DockTree.FindOwningTabNode(item.Id);
                return parent;
            }
        }
    }
}
