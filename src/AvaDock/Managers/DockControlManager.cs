// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
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
    public partial class DockControlManager : ObservableObject
    {
        /// <summary>
        /// The <see cref="DockItemMoveOptions"/> used when moving an item to a newly created floating window.
        /// </summary>
        private static readonly DockItemMoveOptions NewWindowMoveOptions = new()
        {
            DropZone = DropZone.Center,
        };

        /// <summary>Mutatable backing field for <see cref="HiddenItems"/>.</summary>
        private readonly ObservableCollection<DockItemViewModel> hiddenItems = [];

        /// <summary>
        /// Gets or sets the top-level <see cref="DockNodeViewModel"/>.
        /// </summary>
        [ObservableProperty]
        private DockWorkspaceManager primaryWorkspace;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockControlManager"/> class.
        /// </summary>
        /// <param name="rootNode">The root <see cref="DockNodeViewModel"/> for the dock control tree.</param>
        public DockControlManager(DockWorkspaceManager rootNode)
        {
            this.hiddenItems.CollectionChanged += this.OnHiddenItemsChanged;
            this.DockMonitor = new(this.HookItem, this.UnhookItem);

            this.PrimaryWorkspace = rootNode;
            this.PrimaryWorkspace.MinimizedItemsChanged += this.OnWorkspaceMinimizedItemsChanged;
            this.DockMonitor.Monitor(this.PrimaryWorkspace.DockTree);
        }

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently soft-closed and not part of the visuals.
        /// </summary>
        public IEnumerable<DockItemViewModel> HiddenItems => this.hiddenItems;

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently soft-closed and not part of the visuals.
        /// </summary>
        public IEnumerable<DockItemViewModel> Items => this.EnumerateItems();

        /// <summary>
        /// Gets the dictionary to maps each <see cref="DockItemViewModel"/> to its original <see cref="DockTabNodeViewModel"/>.
        /// Used to restore items to the correct tab node when reopened.
        /// </summary>
        public IEnumerable<DockWorkspaceManager> SecondaryWorkspaces => this.WindowManager!.Windows.Select(window => (window.DataContext as DockWorkspaceManager)!);

        /// <summary>Gets or sets the manager for floating widows.</summary>
        // TODO: Refactor so this is private instead of internal.
        internal WindowManager WindowManager { get; set; } = new();

        /// <summary>
        /// Gets the <see cref="DockNodeMonitor"/> used to monitor for changes in <see cref="DockTree"/>s.
        /// </summary>
        private DockNodeMonitor DockMonitor { get; }

        /// <summary>
        /// Attaches the specified <see cref="DockWorkspaceManager"/> to a new floating
        /// window and applies the given bounds.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> to host in a new floating window.</param>
        /// <param name="location">The location to position the window.  If null, the default OS location is used.</param>
        /// <param name="size">The initial size of the floating window.</param>
        /// <returns>The <see cref="DockWorkspaceManager"/> instance that was attached.</returns>
        public IWindow AttachSecondaryWorkspace(DockWorkspaceManager workspace, PixelPoint? location, Size size)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);

            IWindow child = this.WindowManager.CreateWindow();
            child.Content = workspace;
            child.DataContext = workspace;
            child.Width = size.Width;
            child.Height = size.Height;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowInTaskbar = false;

            if (location is not null)
            {
                child.WindowStartupLocation = WindowStartupLocation.Manual;
                child.Position = location.Value;
            }

            // TODO: Refactor so we can unsubscribe.
            workspace.ItemsChanged += (sender, eventArgs) =>
            {
                // If not on the UIThread, the window can be closed while a split
                // is being handled on a single item window.
                Dispatcher.UIThread.Post(
                    () =>
                    {
                        if (child is not null && !workspace.Items.Any())
                        {
                            child.Close();
                            // One would think WindowManager.OnChildClosed would be sufficient, but the sender
                            // is set to the DockWorkspaceManager instance that owns ItemsChanged instead of anything
                            // usable to get back to the IWindow so we have to explicitly remove the window from the
                            // manager as well.
                            this.WindowManager.RemoveWindow(child);
                            workspace.MinimizedItemsChanged -= this.OnWorkspaceMinimizedItemsChanged;
                        }
                    });
            };

            workspace.MinimizedItemsChanged += this.OnWorkspaceMinimizedItemsChanged;

            this.DockMonitor.Monitor(workspace.DockTree);
            return child;
        }

        /// <summary>Closes the <paramref name="item"/>.</summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to close.</param>
        public void CloseItem(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);

            // If the item is already removed, do nothing
            if (!item.DisableClose && !this.Items.Contains(item))
            {
                return;
            }

            DockTabNodeViewModel? tabNode = this.PrimaryWorkspace.DockTree.FindOwningTabNode(item.Id);

            if (tabNode is null)
            {
                foreach (DockWorkspaceManager floating in this.SecondaryWorkspaces)
                {
                    tabNode = floating.DockTree.FindOwningTabNode(item.Id);

                    if (tabNode is not null)
                    {
                        break;
                    }
                }
            }

            _ = tabNode is null
                ? this.hiddenItems.Remove(item)
                : tabNode.RemoveTab(item);

            DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);
        }

        /// <summary>Find an existing <see cref="DockItemViewModel"/> by id.</summary>
        /// <param name="itemId">The id of the <see cref="DockItemViewModel"/> to find.</param>
        /// <returns>The <see cref="DockItemViewModel"/> found or <c>null</c> if no such <see cref="DockItemViewModel"/> exists.</returns>
        public DockItemViewModel? FindItem(String itemId)
        {
            DockItemViewModel? existingItem = this.PrimaryWorkspace.FindItem(itemId);

            if (existingItem is null)
            {
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    existingItem = workspace.FindItem(itemId);

                    if (existingItem is not null)
                    {
                        break;
                    }
                }
            }

            existingItem ??= this.HiddenItems.FirstOrDefault(item => item.Id == itemId);

            return existingItem;
        }

        /// <summary>Find an existing <see cref="DockItemViewModel"/> by id.</summary>
        /// <param name="nodeId">The id of the <see cref="DockNodeViewModel"/> to find.</param>
        /// <returns>The <see cref="DockItemViewModel"/> found or <c>null</c> if no such <see cref="DockItemViewModel"/> exists.</returns>
        public DockNodeViewModel? FindNode(String nodeId)
        {
            DockNodeViewModel? existingNode = this.PrimaryWorkspace.DockTree.FindNode(nodeId);

            if (existingNode is null)
            {
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    existingNode = workspace.DockTree.FindNode(nodeId);

                    if (existingNode is not null)
                    {
                        break;
                    }
                }
            }

            return existingNode;
        }

        /// <summary>Find an existing <see cref="DockItemViewModel"/> by id.</summary>
        /// <param name="itemId">The id of the <see cref="DockItemViewModel"/> whose owning <see cref="DockTabNodeViewModel"/> is to be found.</param>
        /// <returns>The <see cref="DockTabNodeViewModel"/> found or <c>null</c> if no such <see cref="DockTabNodeViewModel"/> exists.</returns>
        public DockTabNodeViewModel? FindOwningTabNode(String itemId)
        {
            DockTabNodeViewModel? existingNode = this.PrimaryWorkspace.DockTree.FindOwningTabNode(itemId);

            if (existingNode is null)
            {
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    existingNode = workspace.DockTree.FindOwningTabNode(itemId);

                    if (existingNode is not null)
                    {
                        break;
                    }
                }
            }

            return existingNode;
        }

        /// <summary>Gets the dimensions for a floating workspace window.</summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/>.</param>
        /// <returns>The <see cref="Rect"/> dimensions for the corresponding <see cref="Window"/> or null
        /// if no such <see cref="Window"/> exists.</returns>
        public Rect? GetFloatingWorkspaceBounds(DockWorkspaceManager workspace)
        {
            IWindow? child = this.WindowManager?
                .Windows
                .FirstOrDefault(window => (window.DataContext as DockWorkspaceManager) == workspace);

            return child?.Bounds;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> that contains the provided <paramref name="root"/>.
        /// </summary>
        /// <param name="root">The <see cref="DockNodeViewModel"/> to look for.</param>
        /// <returns>The found <see cref="DockWorkspaceManager"/>, if any.</returns>
        public DockWorkspaceManager? GetWorkspace(DockNodeViewModel root)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(root);

            if (this.PrimaryWorkspace.DockTree == root)
            {
                return this.PrimaryWorkspace;
            }
            else if (this.PrimaryWorkspace.DockTree.FindNode(root.Id) != null)
            {
                return this.PrimaryWorkspace;
            }
            else
            {
                foreach (IWindow window in this.WindowManager!.Windows)
                {
                    DockWorkspaceManager? workspace = window.DataContext as DockWorkspaceManager;

                    if (workspace?.DockTree == root)
                    {
                        return workspace;
                    }
                    else if (workspace?.DockTree.FindNode(root.Id) != null)
                    {
                        return workspace;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> that contains the provided <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to look for.</param>
        /// <returns>The found <see cref="DockWorkspaceManager"/>, if any.</returns>
        public DockWorkspaceManager? GetWorkspace(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);

            if (this.PrimaryWorkspace.FindItem(item.Id) is not null)
            {
                return this.PrimaryWorkspace;
            }
            else
            {
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    if (workspace.FindItem(item.Id) is not null)
                    {
                        return workspace;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Removes the floating window associated with the specified
        /// <see cref="DockWorkspaceManager"/> and detaches it from management.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> whose floating window should be removed.</param>
        public void RemoveFloatingWorkspace(DockWorkspaceManager workspace)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);

            IWindow? child = this.WindowManager?
                .Windows
                .FirstOrDefault(window => (window.DataContext as DockWorkspaceManager) == workspace);

            this.DockMonitor.Unmonitor(workspace.DockTree);
            child?.Close();
        }

        /// <summary>
        /// Shows all floating windows associated with the current instance.
        /// </summary>
        public void ShowAllWindows()
        {
            this.WindowManager.ShowAll();
        }

        /// <summary>
        /// Adds the specified <see cref="DockItemViewModel"/> to the collection of closed items
        /// and associates it with the given panel ID. This method ensures that the item is properly
        /// hooked for property change notifications so that runtime changes (e.g., reopening)
        /// are handled correctly.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to add as a closed item.</param>
        internal void AddHiddenItem(DockItemViewModel item)
        {
            this.hiddenItems.Add(item);
        }

        /// <summary>
        /// Floats the specified <see cref="DockItemViewModel"/> by detaching it into a new
        /// floating window and positioning it according to the provided coordinates and size.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to float.</param>
        /// <param name="screenLocation">
        /// The top-left position of the floated window in screen coordinates. If <c>null</c>,
        /// the default OS positioning is used.
        /// </param>
        /// <param name="windowSize">
        /// The size of the floating window. If <c>null</c>, a default size will be used.
        /// </param>
        internal void FloatItem(DockItemViewModel item, PixelPoint? screenLocation, Size? windowSize)
        {
            DockTabNodeViewModel? parent = this.FindParentTabNode(item);

            if (parent is not null)
            {
                DockSplitNodeViewModel split = new(Orientation.Horizontal)
                {
                    Id = $"float:{Guid.NewGuid():N}",
                };

                DockTabNodeViewModel floatingTabNode = new();

                split.AddChild(floatingTabNode);

                DockWorkspaceManager newWindowViewModel = new(split);

                windowSize ??= new Size(300, 200);

                IWindow child = this.AttachSecondaryWorkspace(
                    newWindowViewModel,
                    screenLocation,
                    windowSize.Value);

                child.Closing += (_, eventArgs) =>
                {
                    if (eventArgs.CloseReason == WindowCloseReason.WindowClosing)
                    {
                        if (newWindowViewModel.Items.ToList().Any(item => item.DisableClose))
                        {
                            eventArgs.Cancel = true;
                        }
                        else
                        {
                            foreach (DockItemViewModel item in newWindowViewModel.Items.ToList())
                            {
                                if (item.HideCommand.CanExecute(null))
                                {
                                    item.HideCommand.Execute(null);
                                }
                            }
                        }
                    }
                };

                child.Show(this.WindowManager.MainWindow!);
                _ = this.MoveItem(item, floatingTabNode, NewWindowMoveOptions);

                DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);
            }
        }

        /// <summary>
        /// Moves the specified <paramref name="item"/> to a new location in the layout.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> being moved.</param>
        /// <param name="targetNode">The <see cref="DockNodeViewModel"/> that is the drop target for the operation.</param>
        /// <param name="options">Options that describe the drop location and required orientation.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="DockItemViewModel"/> was successfully moved; otherwise, <c>false</c>.
        /// </returns>
        internal Boolean MoveItem(
            DockItemViewModel item,
            DockNodeViewModel targetNode,
            DockItemMoveOptions options)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            TargetFrameworkHelper.ThrowIfArgumentNull(targetNode);
            TargetFrameworkHelper.ThrowIfArgumentNull(options);

            MoveOperation operation = new(this, item, targetNode, options);

            Boolean result = operation.Execute();
            DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);

            return result;
        }

        /// <summary>
        /// Ensures that <see cref="PrimaryWorkspace"/> always has at least one <see cref="DockTabNodeViewModel"/>
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
        /// Enumerates all of the <see cref="DockItemViewModel"/> in the current instance.
        /// </summary>
        /// <returns>The enumeration of <see cref="DockItemViewModel"/>s found.</returns>
        private IEnumerable<DockItemViewModel> EnumerateItems()
        {
            foreach (DockItemViewModel item in this.PrimaryWorkspace.Items)
            {
                yield return item;
            }

            foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
            {
                foreach (DockItemViewModel item in workspace.Items)
                {
                    yield return item;
                }
            }

            foreach (DockItemViewModel item in this.HiddenItems)
            {
                yield return item;
            }
        }

        /// <summary>Finds which, if any, owned <see cref="DockTabNodeViewModel"/> contains the given <see cref="DockItemViewModel"/>.</summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> whose owner we want to find.</param>
        /// <returns>The root found, if any.</returns>
        private DockTabNodeViewModel? FindParentTabNode(DockItemViewModel item)
        {
            DockTabNodeViewModel? parent = this.PrimaryWorkspace.DockTree.FindOwningTabNode(item.Id);

            if (parent is not null)
            {
                return parent;
            }
            else
            {
                foreach (DockWorkspaceManager floatingwindow in this.SecondaryWorkspaces)
                {
                    parent = floatingwindow.DockTree.FindOwningTabNode(item.Id);
                    if (parent is not null)
                    {
                        return parent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current <see cref="DockControlManager"/>
        /// will be notified of changes it needs to update state based on.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to proces.</param>
        private void HookItem(DockItemViewModel item)
        {
            item.CloseRequested += this.OnItemCloseRequested;
            item.HideRequested += this.OnItemHideRequested;
            item.ShowRequested += this.OnItemShowRequested;
            DockContext.SetDockHost(item, this);
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

            // Reset - collection replaced (rare, but possible)
            if (eventArgs.Action is NotifyCollectionChangedAction.Reset)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems ?? Array.Empty<DockItemViewModel>())
                {
                    this.UnhookItem(item);
                }

                foreach (DockItemViewModel item in this.hiddenItems)
                {
                    this.HookItem(item);
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be closed
        /// from the hidden state back to its original <see cref="DockTabNodeViewModel"/> panel.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested closing.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the restore request.</param>
        private void OnItemCloseRequested(Object? sender, DockItemCloseRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;

            if (this.hiddenItems.Contains(item))
            {
                _ = this.hiddenItems.Remove(item);
            }
            else
            {
                DockTabNodeViewModel? tabNode = this.FindParentTabNode(item);
                System.Diagnostics.Debug.Assert(tabNode is not null, "Couldn't find item being closed.");

                if (tabNode is not null)
                {
                    DockWorkspaceManager? workspace = this.GetWorkspace(tabNode);
                    _ = tabNode.RemoveTab(item);
                    workspace?.CommitChanges();
                }
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be hide in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be hidden.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the show request.</param>
        private void OnItemHideRequested(Object? sender, DockItemHideRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;

            if (!this.hiddenItems.Contains(item))
            {
                DockTabNodeViewModel? tabNode = this.FindParentTabNode(item);
                System.Diagnostics.Debug.Assert(tabNode is not null, "Couldn't find item being hidden.");

                if (tabNode is not null)
                {
                    DockWorkspaceManager? workspace = this.GetWorkspace(tabNode);
                    System.Diagnostics.Debug.Assert(workspace is not null, "Couldn't find workspace for owned item.");

                    if (workspace is not null)
                    {
                        DockContext.SetPreferredWorkspaceId(item, workspace.Id);

                        if (workspace.RemoveItem(item))
                        {
                            this.hiddenItems.Add(item);
                            this.OnPropertyChanged(nameof(this.HiddenItems));
                            workspace.CommitChanges();
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(!this.hiddenItems.Contains(item), "Can't re-hide a hidden item.");
            }

            DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be show in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be shown.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the show request.</param>
        private void OnItemShowRequested(Object? sender, DockItemShowRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;

            if (this.hiddenItems.Contains(item))
            {
                String? workspaceId = DockContext.GetPreferredWorkspaceId(item);
                DockWorkspaceManager workspace = this.PrimaryWorkspace;

                if (workspaceId is not null && workspaceId != this.PrimaryWorkspace.Id)
                {
                    foreach (DockWorkspaceManager floatingWorkspace in this.SecondaryWorkspaces.ToList())
                    {
                        if (floatingWorkspace.Id == workspaceId)
                        {
                            workspace = floatingWorkspace;
                            break;
                        }
                    }
                }

                if (this.hiddenItems.Remove(item))
                {
                    Boolean added = workspace.AddItem(item);
                    System.Diagnostics.Debug.Assert(added, "Failed to add item to workspace during show request.");

                    this.OnPropertyChanged(nameof(this.HiddenItems));
                    DockContext.ClearPreferredWorkspaceId(item);
                    workspace.CommitChanges();
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(this.hiddenItems.Contains(item), "Can't show an item that isn't hidden.");
            }
        }

        /// <summary>
        /// Handles changes to a workspace's <see cref="DockWorkspaceManager.MinimizedItems"/> collection.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Object"/> that raised the event (the <see cref="ObservableCollection{T}"/>).
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> containing details about which items were
        /// added, removed, or reset.
        /// </param>
        private void OnWorkspaceMinimizedItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (sender is DockWorkspaceManager workspace)
            {
                DockControlManager.EnsureWorkspaceHasTabNode(workspace);
            }
        }

        /// <summary>
        /// Handles removing all handlers for a <see cref="DockItemViewModel"/> so the current <see cref="DockWorkspaceManager"/>
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Placeholder method.")]
        private void UnhookItem(DockItemViewModel item)
        {
            item.CloseRequested -= this.OnItemCloseRequested;
            item.HideRequested -= this.OnItemHideRequested;
            item.ShowRequested -= this.OnItemShowRequested;
        }

        /// <summary>
        /// Encapsulates the logic for moving a <see cref="DockItemViewModel"/> within the dock layout.
        /// </summary>
        private sealed class MoveOperation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MoveOperation"/> class.
            /// </summary>
            /// <param name="owner">The <see cref="DockLayoutManager"/> that owns this operation.</param>
            /// <param name="item">The <see cref="DockItemViewModel"/> being moved.</param>
            /// <param name="targetNode">The target node to which the <see cref="DockItemViewModel"/> is being moved.</param>
            /// <param name="options">The move options describing drop location and orientation.</param>
            public MoveOperation(
                DockControlManager owner,
                DockItemViewModel item,
                DockNodeViewModel targetNode,
                DockItemMoveOptions options)
            {
                this.Owner = owner;
                this.Item = item;
                this.TargetNode = targetNode;
                this.Options = options;
            }

            /// <summary>
            /// Gets the move options describing drop location and orientation.
            /// </summary>
            private DockItemMoveOptions Options { get; }

            /// <summary>
            /// Gets the <see cref="DockLayoutManager"/> that owns this operation.
            /// </summary>
            private DockControlManager Owner { get; }

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
                DockTabNodeViewModel sourceTabNode = this.Owner.FindParentTabNode(this.Item)!;

                if (this.Options.DropZone == DropZone.None || (sourceTabNode == this.TargetNode && sourceTabNode.Tabs.Count == 1))
                {
                    result = true; // No-op move
                }
                else
                {
                    result = this.Options.DropZone == DropZone.Center
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

                Orientation splitOrientation = operation.Options.DropZone switch
                {
                    DropZone.Left or DropZone.Right => Orientation.Horizontal,
                    DropZone.Top or DropZone.Bottom => Orientation.Vertical,
                    DropZone.Center or DropZone.None => throw new InvalidOperationException("Unsupported drop zone."),
                    _ => throw new InvalidOperationException("Unsupported drop zone."),
                };

                DockSplitNodeViewModel wrappedSplit = new(splitOrientation);
                if (operation.Options.DropZone is DropZone.Left or DropZone.Top)
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

                System.Diagnostics.Debug.Assert(operation.Options.DropZone == DropZone.Center, "Invalid code path.");

                DockWorkspaceManager sourceWorkspace = operation.Owner.GetWorkspace(sourceTabNode)!;
                DockWorkspaceManager? destinationWorkspace = operation.Owner.GetWorkspace(operation.TargetNode);

                if (operation.TargetNode is DockTabNodeViewModel targetTabNode)
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

                DockWorkspaceManager sourceWorkspace = operation.Owner.GetWorkspace(sourceTabNode)!;
                DockWorkspaceManager? destinationWorkspace = operation.Owner.GetWorkspace(operation.TargetNode);
                DockSplitNodeViewModel wrappedSplit = MoveOperation.CreateSplit(operation);
                DockSplitNodeViewModel? parentSplit = destinationWorkspace?.DockTree.GetContainingSplit(operation.TargetNode);

                if (parentSplit is not null)
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
                DockTabNodeViewModel? sourceTabNode = operation.Owner.FindParentTabNode(operation.Item);
                if (sourceTabNode is null)
                {
                    return false;
                }

                DockWorkspaceManager? workspace = operation.Owner.GetWorkspace(sourceTabNode);
                return workspace is not null;
            }
        }
    }
}
