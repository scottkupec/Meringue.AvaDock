// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
            this.PrimaryWorkspace.MinimizedItemsChanged += this.HandleWorkspaceMinimizedItemsChanged;
            this.PrimaryWorkspace.ItemMinimized += this.HandleWorkspaceItemMinimized;
            this.PrimaryWorkspace.ItemMinimizing += this.HandleWorkspaceItemMinimizing;
            this.PrimaryWorkspace.ItemRestored += this.HandleWorkspaceItemRestored;
            this.PrimaryWorkspace.ItemRestoring += this.HandleWorkspaceItemRestoring;
            this.DockMonitor.Monitor(this.PrimaryWorkspace.DockTree);
        }

        /// <summary>
        /// Occurs when the layout of the dock control has changed.
        /// </summary>
        public event EventHandler<LayoutChangedEventArgs>? LayoutChanged;

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
        public event EventHandler<DockItemHideRequestedEventArgs>? ItemHidden;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being hidden
        /// </summary>
        public event EventHandler<DockItemHideRequestedEventArgs>? ItemHiding;

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
        public event EventHandler<DockItemMoveRequestedEventArgs>? ItemMoved;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being moved.
        /// </summary>
        public event EventHandler<DockItemMoveRequestedEventArgs>? ItemMoving;

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
        public event EventHandler<DockItemShowRequestedEventArgs>? ItemShown;

        /// <summary>
        /// Occurs when a <see cref="DockItemViewModel"/> is being shown (restored from hidden state).
        /// </summary>
        public event EventHandler<DockItemShowRequestedEventArgs>? ItemShowing;

        /// <summary>
        /// Occurs when a secondary <see cref="DockWorkspaceManager"/> has been attached.
        /// </summary>
        public event EventHandler<DockWorkspaceAttachedEventArgs>? WorkspaceAttached;

        /// <summary>
        /// Occurs when a secondary <see cref="DockWorkspaceManager"/> is being attached.
        /// </summary>
        public event EventHandler<DockWorkspaceAttachingEventArgs>? WorkspaceAttaching;

        /// <summary>
        /// Occurs when a secondary <see cref="DockWorkspaceManager"/> has been detached.
        /// </summary>
        public event EventHandler<DockWorkspaceDetachedEventArgs>? WorkspaceDetached;

        /// <summary>
        /// Occurs when a secondary <see cref="DockWorkspaceManager"/> is being detached.
        /// </summary>
        public event EventHandler<DockWorkspaceDetachingEventArgs>? WorkspaceDetaching;

        /// <summary>
        /// Occurs when items are added to or removed from the items collection
        /// across all managed <see cref="DockWorkspaceManager"/> instances.
        /// </summary>
        /// <remarks>
        /// This event aggregates changes from both the primary workspace and all secondary (floating) workspaces.
        /// Use this event if you want to be notified about any item change regardless of which workspace
        /// the item belongs to. For workspace-specific changes, subscribe to the <see cref="DockWorkspaceManager.MinimizedItemsChanged"/>
        /// event on individual workspaces.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? ItemsChanged
        {
            add
            {
                this.itemsChanged += value;
                // When adding a subscriber, also subscribe to all current workspaces
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    workspace.ItemsChanged += this.HandleWorkspaceItemsChanged;
                }
            }

            remove
            {
                this.itemsChanged -= value;
                // When removing a subscriber, unsubscribe from all workspaces if no subscribers left
                if (this.itemsChanged is null)
                {
                    foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                    {
                        workspace.ItemsChanged -= this.HandleWorkspaceItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when items are added to or removed from the minimized items collection
        /// across all managed <see cref="DockWorkspaceManager"/> instances.
        /// </summary>
        /// <remarks>
        /// This event aggregates changes from both the primary workspace and all secondary (floating) workspaces.
        /// Use this event if you want to be notified about any minimized item change regardless of which workspace
        /// the item belongs to. For workspace-specific changes, subscribe to the <see cref="DockWorkspaceManager.MinimizedItemsChanged"/>
        /// event on individual workspaces.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? MinimizedItemsChanged
        {
            add
            {
                this.minimizedItemsChanged += value;
                // When adding a subscriber, also subscribe to all current workspaces
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    workspace.MinimizedItemsChanged += this.HandleWorkspaceMinimizedItemsChanged;
                }
            }

            remove
            {
                this.minimizedItemsChanged -= value;
                // When removing a subscriber, unsubscribe from all workspaces if no subscribers left
                if (this.minimizedItemsChanged is null)
                {
                    foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                    {
                        workspace.MinimizedItemsChanged -= this.HandleWorkspaceMinimizedItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for the <see cref="ItemsChanged"/> event. This is used to manage subscriptions and forward
        /// events from individual workspaces.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Private backing event.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private backing event.")]
        private event NotifyCollectionChangedEventHandler? itemsChanged;

        /// <summary>
        /// Backing field for the <see cref="MinimizedItemsChanged"/> event. This is used to manage subscriptions and forward
        /// events from individual workspaces.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Private backing event.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private backing event.")]
        private event NotifyCollectionChangedEventHandler? minimizedItemsChanged;

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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IEnumerable<DockWorkspaceManager> SecondaryWorkspaces => this.WindowManager!.Windows.Select(window => (window.DataContext as DockWorkspaceManager)!);

        /// <summary>Gets or sets the manager for floating widows.</summary>
        // TODO: Refactor so this is private instead of internal.
        internal WindowManager WindowManager { get; set; } = new(new AvaloniaWindowFactory());

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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public IWindow? AttachSecondaryWorkspace(DockWorkspaceManager workspace, PixelPoint? location, Size size)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(workspace);

            if (!this.OnWorkspaceAttaching(workspace))
            {
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
                                workspace.MinimizedItemsChanged -= this.HandleWorkspaceMinimizedItemsChanged;
                                workspace.ItemsChanged -= this.HandleWorkspaceItemsChanged;
                                workspace.ItemMinimized -= this.HandleWorkspaceItemMinimized;
                                workspace.ItemMinimizing -= this.HandleWorkspaceItemMinimizing;
                                workspace.ItemRestored -= this.HandleWorkspaceItemRestored;
                                workspace.ItemRestoring -= this.HandleWorkspaceItemRestoring;
                            }
                        });
                };

                child.Closing += (sender, closingEventArgs) =>
                {
                    Boolean cancelDetach = this.OnWorkspaceDetaching(workspace);
                    closingEventArgs.Cancel = closingEventArgs.Cancel && cancelDetach;
                };

                child.Closed += (sender, eventArgs) => this.OnWorkspaceDetached(workspace);

                workspace.MinimizedItemsChanged += this.HandleWorkspaceMinimizedItemsChanged;
                workspace.ItemsChanged += this.HandleWorkspaceItemsChanged;
                workspace.ItemMinimized += this.HandleWorkspaceItemMinimized;
                workspace.ItemMinimizing += this.HandleWorkspaceItemMinimizing;
                workspace.ItemRestored += this.HandleWorkspaceItemRestored;
                workspace.ItemRestoring += this.HandleWorkspaceItemRestoring;

                this.DockMonitor.Monitor(workspace.DockTree);
                this.OnWorkspaceAttached(workspace);
                return child;
            }
            else
            {
                return null;
            }
        }

        /// <summary>Closes the <paramref name="item"/>.</summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to close.</param>
        public void CloseItem(DockItemViewModel item)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);

            // If the item is already closed or not allowed to be closed, do nothing
            if (item.DisableClose || !this.Items.Contains(item))
            {
                return;
            }

            this.HandleItemCloseRequested(this, new DockItemCloseRequestedEventArgs(item));
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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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
                foreach (IWindow window in this.WindowManager.Windows)
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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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
        [EditorBrowsable(EditorBrowsableState.Advanced)]
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

                IWindow? child = this.AttachSecondaryWorkspace(
                    newWindowViewModel,
                    screenLocation,
                    windowSize.Value);

                if (child is not null)
                {
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

                    child.Show(this.WindowManager.MainWindow);
                    _ = this.MoveItem(item, floatingTabNode, NewWindowMoveOptions);

                    DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);
                }
                else
                {
                }
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
        internal Boolean MoveItem(DockItemViewModel item, DockNodeViewModel targetNode, DockItemMoveOptions options)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(item);
            TargetFrameworkHelper.ThrowIfArgumentNull(targetNode);
            TargetFrameworkHelper.ThrowIfArgumentNull(options);

            DockTabNodeViewModel? sourceTabNode = this.FindParentTabNode(item);

            DockItemMoveRequestedEventArgs eventArgs = new(item, sourceTabNode, targetNode);
            Boolean operationPermitted = !this.OnItemMoving(this, eventArgs);

            if (operationPermitted)
            {
                MoveOperation operation = new(this, item, targetNode, options);

                Boolean result = operation.Execute();
                DockControlManager.EnsureWorkspaceHasTabNode(this.PrimaryWorkspace);
                return result;
            }
            else
            {
                return false;
            }
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
        /// Handles a request from a <see cref="DockItemViewModel"/> to be closed
        /// from the hidden state back to its original <see cref="DockTabNodeViewModel"/> panel.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested closing.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the restore request.</param>
        private void HandleItemCloseRequested(Object? sender, DockItemCloseRequestedEventArgs eventArgs)
        {
            DockItemViewModel item = eventArgs.Item;
            Boolean operationPermitted = !this.OnItemClosing(item);

            if (operationPermitted)
            {
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

                this.OnItemClosed(item);
            }
        }

        /// <summary>
        /// Handles items changes in individual workspaces and forwards to the aggregate event.
        /// </summary>
        /// <param name="sender">The workspace whose items changed.</param>
        /// <param name="eventArgs">Information about the change.</param>
        private void HandleWorkspaceItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.itemsChanged?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handles minimized items changes in individual workspaces and forwards to the aggregate event.
        /// </summary>
        /// <param name="sender">The workspace whose minimized items changed.</param>
        /// <param name="eventArgs">Information about the change.</param>
        private void HandleWorkspaceMinimizedItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.minimizedItemsChanged?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current <see cref="DockControlManager"/>
        /// will be notified of changes it needs to update state based on.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to proces.</param>
        private void HookItem(DockItemViewModel item)
        {
            item.CloseRequested += this.HandleItemCloseRequested;
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
        /// Raises the <see cref="ItemClosed"/> event.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was attached.</param>
        private void OnItemClosed(DockItemViewModel item)
        {
            this.ItemClosed?.Invoke(this, new DockItemClosedEventArgs(item));
        }

        /// <summary>
        /// Raises the <see cref="ItemClosing"/> event when a workspace is in the process of being attached.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being attached.</param>
        /// <returns><c>true</c> if the close should be canceled; otherwise, <c>false</c>.</returns>
        private Boolean OnItemClosing(DockItemViewModel item)
        {
            DockItemClosingEventArgs eventArgs = new(item);
            this.ItemClosing?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="ItemHidden"/> event when an item is hidden.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was attached.</param>
        private void OnItemHidden(DockItemViewModel item)
        {
            this.ItemHidden?.Invoke(this, new DockItemHideRequestedEventArgs(item));
        }

        /// <summary>
        /// Raises the <see cref="ItemHiding"/> event when a item is being hidden.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being attached.</param>
        /// <returns><c>true</c> if the close should be canceled; otherwise, <c>false</c>.</returns>
        private Boolean OnItemHiding(DockItemViewModel item)
        {
            DockItemHideRequestedEventArgs eventArgs = new(item);
            this.ItemHiding?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
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
                        Boolean operationPermitted = !this.OnItemHiding(item);

                        if (operationPermitted)
                        {
                            DockContext.SetPreferredWorkspaceId(item, workspace.Id);

                            if (workspace.RemoveItem(item))
                            {
                                this.hiddenItems.Add(item);
                                this.OnPropertyChanged(nameof(this.HiddenItems));
                                workspace.CommitChanges();
                                this.OnItemHidden(item);
                            }
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
        /// Raises the <see cref="ItemMinimized"/> event when an item is being minimized.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemMinimizedEventArgs"/> for the event.</param>
        private void HandleWorkspaceItemMinimized(Object? sender, DockItemMinimizedEventArgs eventArgs)
        {
            if (sender is DockWorkspaceManager workspace)
            {
                DockControlManager.EnsureWorkspaceHasTabNode(workspace);
            }

            this.ItemMinimized?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ItemRestoring"/> event when an item is in the process of being minimized.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemMinimizingEventArgs"/> for the event.</param>
        private void HandleWorkspaceItemMinimizing(Object? sender, DockItemMinimizingEventArgs eventArgs)
        {
            this.ItemMinimizing?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ItemMoved"/> event when an item has been moved.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemMoveRequestedEventArgs"/> for the event.</param>
        private void OnItemMoved(Object? sender, DockItemMoveRequestedEventArgs eventArgs)
        {
            if (sender is DockWorkspaceManager workspace)
            {
                DockControlManager.EnsureWorkspaceHasTabNode(workspace);
            }

            this.ItemMoved?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ItemMoving"/> event when an item is in the process of being moved.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemMoveRequestedEventArgs"/> for the event.</param>
        /// <returns><c>true</c> if the close should be canceled; otherwise, <c>false</c>.</returns>
        private Boolean OnItemMoving(Object? sender, DockItemMoveRequestedEventArgs eventArgs)
        {
            this.ItemMoving?.Invoke(sender, eventArgs);
            return eventArgs.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="ItemRestored"/> event when an item is restored from the minimized state.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemRestoreRequestedEventArgs"/> for the event.</param>
        private void HandleWorkspaceItemRestored(Object? sender, DockItemRestoredEventArgs eventArgs)
        {
            this.ItemRestored?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ItemRestoring"/> event when an item is in the process of being restored.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The <see cref="DockItemRestoreRequestedEventArgs"/> for the event.</param>
        private void HandleWorkspaceItemRestoring(Object? sender, DockItemRestoringEventArgs eventArgs)
        {
            this.ItemRestoring?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ItemShown"/> event.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that was attached.</param>
        private void OnItemShown(DockItemViewModel item)
        {
            this.ItemShown?.Invoke(this, new DockItemShowRequestedEventArgs(item));
        }

        /// <summary>
        /// Raises the <see cref="ItemShowing"/> event when a workspace is in the process of being attached.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is being attached.</param>
        /// <returns><c>true</c> if the close should be canceled; otherwise, <c>false</c>.</returns>
        private Boolean OnItemShowing(DockItemViewModel item)
        {
            DockItemShowRequestedEventArgs eventArgs = new(item);
            this.ItemShowing?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
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

                Boolean operationPermitted = !this.OnItemShowing(item);

                if (operationPermitted)
                {
                    if (this.hiddenItems.Remove(item))
                    {
                        Boolean added = workspace.AddItem(item);
                        System.Diagnostics.Debug.Assert(added, "Failed to add item to workspace during show request.");

                        this.OnPropertyChanged(nameof(this.HiddenItems));
                        DockContext.ClearPreferredWorkspaceId(item);
                        workspace.CommitChanges();
                        this.OnItemShown(item);
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(this.hiddenItems.Contains(item), "Can't show an item that isn't hidden.");
            }
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be show in the UI.
        /// </summary>
        /// <param name="args">The <see cref="LayoutChangedEventArgs"/> for the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "WIP")]
        private void OnLayoutChanged(LayoutChangedEventArgs args)
        {
            this.LayoutChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceAttached"/> event.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was attached.</param>
        private void OnWorkspaceAttached(DockWorkspaceManager workspace)
        {
            this.WorkspaceAttached?.Invoke(this, new DockWorkspaceAttachedEventArgs(workspace));
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceAttached"/> event when a workspace is in the process of being attached.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that is being attached.</param>
        /// <returns><c>true</c> if the attachment should be canceled; otherwise, <c>false</c>.</returns>
        private Boolean OnWorkspaceAttaching(DockWorkspaceManager workspace)
        {
            DockWorkspaceAttachingEventArgs eventArgs = new(workspace);
            this.WorkspaceAttaching?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceDetached"/> event.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> that was detached.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "WIP")]
        private void OnWorkspaceDetached(DockWorkspaceManager workspace)
        {
            this.WorkspaceDetached?.Invoke(this, new DockWorkspaceDetachedEventArgs(workspace));
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceDetached"/> event.
        /// </summary>
        /// <param name="workspace">The <see cref="DockWorkspaceManager"/> is being detached.</param>
        /// <returns><c>true</c> if the detachment should be canceled; otherwise, <c>false</c>.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "WIP")]
        private Boolean OnWorkspaceDetaching(DockWorkspaceManager workspace)
        {
            DockWorkspaceDetachingEventArgs eventArgs = new(workspace);
            this.WorkspaceDetaching?.Invoke(this, eventArgs);
            return eventArgs.Cancel;
        }

        /// <summary>
        /// Handles removing all handlers for a <see cref="DockItemViewModel"/> so the current <see cref="DockWorkspaceManager"/>
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Placeholder method.")]
        private void UnhookItem(DockItemViewModel item)
        {
            item.CloseRequested -= this.HandleItemCloseRequested;
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
