// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
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
            this.DockMonitor = new(this.HookItem, this.UnhookItem);

            this.PrimaryWorkspace = rootNode;
            this.PrimaryWorkspace.HiddenItemsChanged += this.ForwardHiddenItemsChanged;
            this.PrimaryWorkspace.ItemsChanged += this.ForwardItemsChanged;
            this.PrimaryWorkspace.MinimizedItemsChanged += this.ForwardMinimizedItemsChanged;
            this.PrimaryWorkspace.ItemClosed += this.ForwardItemClosed;
            this.PrimaryWorkspace.ItemClosing += this.ForwardItemClosing;
            this.PrimaryWorkspace.ItemHidden += this.ForwardItemHidden;
            this.PrimaryWorkspace.ItemHiding += this.ForwardItemHiding;
            this.PrimaryWorkspace.ItemMinimized += this.ForwardItemMinimized;
            this.PrimaryWorkspace.ItemMinimizing += this.ForwardItemMinimizing;
            this.PrimaryWorkspace.ItemMoved += this.ForwardItemMoved;
            this.PrimaryWorkspace.ItemMoving += this.ForwardItemMoving;
            this.PrimaryWorkspace.ItemRestored += this.ForwardItemRestored;
            this.PrimaryWorkspace.ItemRestoring += this.ForwardItemRestoring;
            this.PrimaryWorkspace.ItemShowing += this.ForwardItemShowing;
            this.PrimaryWorkspace.ItemShown += this.ForwardItemShown;

            this.DockMonitor.Monitor(this.PrimaryWorkspace.DockTree);
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
        /// Occurs when the layout of the dock control has changed.
        /// </summary>
        public event EventHandler<LayoutChangedEventArgs>? LayoutChanged;

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
        public event NotifyCollectionChangedEventHandler? HiddenItemsChanged
        {
            add
            {
                this.itemsChanged += value;
                // When adding a subscriber, also subscribe to all current workspaces
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    workspace.HiddenItemsChanged += this.ForwardHiddenItemsChanged;
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
                        workspace.HiddenItemsChanged -= this.ForwardHiddenItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when items are added to or removed from the items collection
        /// across all managed <see cref="DockWorkspaceManager"/> instances.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? ItemsChanged
        {
            add
            {
                this.itemsChanged += value;
                // When adding a subscriber, also subscribe to all current workspaces
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    workspace.ItemsChanged += this.ForwardItemsChanged;
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
                        workspace.ItemsChanged -= this.ForwardItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Occurs when items are added to or removed from the minimized items collection
        /// across all managed <see cref="DockWorkspaceManager"/> instances.
        /// </summary>
        public event NotifyCollectionChangedEventHandler? MinimizedItemsChanged
        {
            add
            {
                this.minimizedItemsChanged += value;
                // When adding a subscriber, also subscribe to all current workspaces
                foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
                {
                    workspace.MinimizedItemsChanged += this.ForwardMinimizedItemsChanged;
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
                        workspace.MinimizedItemsChanged -= this.ForwardMinimizedItemsChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Backing field for the <see cref="HiddenItemsChanged"/> event. This is used to manage subscriptions and forward
        /// events from individual workspaces.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Private backing event.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Private backing event.")]
        private event NotifyCollectionChangedEventHandler? hiddenItemsChanged;

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
        public IEnumerable<DockItemViewModel> HiddenItems => this.EnumerateHiddenItems();

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently soft-closed and not part of the visuals.
        /// </summary>
        public IEnumerable<DockItemViewModel> Items => this.EnumerateItems();

        /// <summary>
        /// Gets the list of <see cref="DockItemViewModel"/> that are currently minimized.
        /// </summary>
        public IEnumerable<DockItemViewModel> MinimizedItems => this.EnumerateMinimizedItems();

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
            DockWorkspaceAttachingEventArgs attachingEventArgs = new(workspace);
            this.OnWorkspaceAttaching(attachingEventArgs);

            if (!attachingEventArgs.Cancel)
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
                                workspace.HiddenItemsChanged -= this.ForwardHiddenItemsChanged;
                                workspace.ItemsChanged -= this.ForwardItemsChanged;
                                workspace.ItemClosed -= this.ForwardItemClosed;
                                workspace.ItemClosing -= this.ForwardItemClosing;
                                workspace.ItemHidden -= this.ForwardItemHidden;
                                workspace.ItemHiding -= this.ForwardItemHiding;
                                workspace.ItemMinimized -= this.ForwardItemMinimized;
                                workspace.ItemMinimizing -= this.ForwardItemMinimizing;
                                workspace.ItemMoved -= this.ForwardItemMoved;
                                workspace.ItemMoving -= this.ForwardItemMoving;
                                workspace.ItemRestored -= this.ForwardItemRestored;
                                workspace.ItemRestoring -= this.ForwardItemRestoring;
                                workspace.ItemShowing -= this.ForwardItemShowing;
                                workspace.ItemShown -= this.ForwardItemShown;
                                workspace.MinimizedItemsChanged -= this.ForwardMinimizedItemsChanged;
                            }
                        });
                };

                child.Closing += (sender, closingEventArgs) =>
                {
                    DockWorkspaceDetachingEventArgs detachingEventArgs = new(workspace);
                    this.OnWorkspaceDetaching(detachingEventArgs);
                    closingEventArgs.Cancel = closingEventArgs.Cancel || detachingEventArgs.Cancel;
                };

                child.Closed += (sender, eventArgs) => this.OnWorkspaceDetached(new DockWorkspaceDetachedEventArgs(workspace));

                workspace.HiddenItemsChanged += this.ForwardHiddenItemsChanged;
                workspace.ItemsChanged += this.ForwardItemsChanged;
                workspace.ItemClosed += this.ForwardItemClosed;
                workspace.ItemClosing += this.ForwardItemClosing;
                workspace.ItemHidden += this.ForwardItemHidden;
                workspace.ItemHiding += this.ForwardItemHiding;
                workspace.ItemMinimized += this.ForwardItemMinimized;
                workspace.ItemMinimizing += this.ForwardItemMinimizing;
                workspace.ItemMoved += this.ForwardItemMoved;
                workspace.ItemMoving += this.ForwardItemMoving;
                workspace.ItemRestored += this.ForwardItemRestored;
                workspace.ItemRestoring += this.ForwardItemRestoring;
                workspace.ItemShowing += this.ForwardItemShowing;
                workspace.ItemShown += this.ForwardItemShown;
                workspace.MinimizedItemsChanged += this.ForwardMinimizedItemsChanged;

                this.DockMonitor.Monitor(workspace.DockTree);
                this.OnWorkspaceAttached(new DockWorkspaceAttachedEventArgs(workspace));
                return child;
            }
            else
            {
                return null;
            }
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

            foreach (DockItemViewModel item in workspace.HiddenItems)
            {
                _ = this.PrimaryWorkspace.AddItem(item);
            }

            foreach (DockItemViewModel item in workspace.Items)
            {
                _ = this.PrimaryWorkspace.AddItem(item);
            }

            foreach (DockItemViewModel item in workspace.MinimizedItems)
            {
                _ = this.PrimaryWorkspace.AddItem(item);
            }

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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Refactor in progress.")]
        internal void AddHiddenItem(DockItemViewModel item)
        {
            DockContext.SetItemState(item, DockItemState.Hidden);
            _ = this.PrimaryWorkspace.AddItem(item);
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
                DockWorkspaceManager newWorkspace = DockControlManager.CreateMinimalWorkspace();

                windowSize ??= new Size(300, 200);

                IWindow? child = this.AttachSecondaryWorkspace(
                    newWorkspace,
                    screenLocation,
                    windowSize.Value);

                if (child is not null)
                {
                    child.Closing += (_, eventArgs) =>
                    {
                        if (eventArgs.CloseReason == WindowCloseReason.WindowClosing)
                        {
                            if (newWorkspace.Items.ToList().Any(item => item.DisableClose))
                            {
                                eventArgs.Cancel = true;
                            }
                            else
                            {
                                foreach (DockItemViewModel item in newWorkspace.Items.ToList())
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
                    item.RequestMove(parent, newWorkspace.DockTree.FindFirstTabNode()!, MovePlacement.On, null);
                }
                else
                {
                }
            }
        }

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
        /// Handles a request from a <see cref="DockItemViewModel"/> to be show in the UI.
        /// </summary>
        /// <param name="eventArgs">The <see cref="LayoutChangedEventArgs"/> for the event.</param>
        protected virtual void OnLayoutChanged(LayoutChangedEventArgs eventArgs) => this.LayoutChanged?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="WorkspaceAttached"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockWorkspaceAttachedEventArgs"/> for the event.</param>
        protected virtual void OnWorkspaceAttached(DockWorkspaceAttachedEventArgs eventArgs)
        {
            this.WorkspaceAttached?.Invoke(this, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceAttached"/> event when a workspace is in the process of being attached.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockWorkspaceAttachingEventArgs"/> for the event.</param>
        protected virtual void OnWorkspaceAttaching(DockWorkspaceAttachingEventArgs eventArgs) => this.WorkspaceAttaching?.Invoke(this, eventArgs);

        /// <summary>
        /// Raises the <see cref="WorkspaceDetached"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockWorkspaceDetachedEventArgs"/> for the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "WIP")]
        protected virtual void OnWorkspaceDetached(DockWorkspaceDetachedEventArgs eventArgs)
        {
            this.WorkspaceDetached?.Invoke(this, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Raises the <see cref="WorkspaceDetached"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DockWorkspaceDetachingEventArgs"/> for the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "WIP")]
        protected virtual void OnWorkspaceDetaching(DockWorkspaceDetachingEventArgs eventArgs) => this.WorkspaceDetaching?.Invoke(this, eventArgs);

        /// <summary>
        /// Helper method to initialize a new <see cref="DockWorkspaceManager"/>.
        /// </summary>
        /// <param name="id">If provided, the value for the new workspace's ID property. If null,
        /// the new workspace will be created with a random ID.</param>
        /// <returns>The new <see cref="DockWorkspaceManager"/>.</returns>
        private static DockWorkspaceManager CreateMinimalWorkspace(String? id = null)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = $"{Guid.NewGuid():N}";
            }

            DockSplitNodeViewModel split = new(Orientation.Horizontal)
            {
                Id = $"float:{id}",
            };

            DockTabNodeViewModel floatingTabNode = new();
            split.AddChild(floatingTabNode);
            return new DockWorkspaceManager(split);
        }

        /// <summary>
        /// Enumerates all of the <see cref="DockItemViewModel"/> in the current instance.
        /// </summary>
        /// <returns>The enumeration of <see cref="DockItemViewModel"/>s found.</returns>
        private IEnumerable<DockItemViewModel> EnumerateHiddenItems()
        {
            foreach (DockItemViewModel item in this.PrimaryWorkspace.HiddenItems)
            {
                yield return item;
            }

            foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
            {
                foreach (DockItemViewModel item in workspace.HiddenItems)
                {
                    yield return item;
                }
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

        /// <summary>
        /// Enumerates all of the hidden <see cref="DockItemViewModel"/> in the current instance.
        /// </summary>
        /// <returns>The enumeration of <see cref="DockItemViewModel"/>s found.</returns>
        private IEnumerable<DockItemViewModel> EnumerateMinimizedItems()
        {
            foreach (DockItemViewModel item in this.PrimaryWorkspace.MinimizedItems)
            {
                yield return item;
            }

            foreach (DockWorkspaceManager workspace in this.SecondaryWorkspaces)
            {
                foreach (DockItemViewModel item in workspace.MinimizedItems)
                {
                    yield return item;
                }
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
        /// Handles hidden items changes in individual workspaces and forwards to the aggregate event.
        /// </summary>
        /// <param name="sender">The workspace whose items changed.</param>
        /// <param name="eventArgs">Information about the change.</param>
        private void ForwardHiddenItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs) => this.hiddenItemsChanged?.Invoke(sender, eventArgs);

        /// <summary>
        /// Handles items changes in individual workspaces and forwards to the aggregate event.
        /// </summary>
        /// <param name="sender">The workspace whose items changed.</param>
        /// <param name="eventArgs">Information about the change.</param>
        private void ForwardItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs) => this.itemsChanged?.Invoke(sender, eventArgs);

        /// <summary>
        /// Handles minimized items changes in individual workspaces and forwards to the aggregate event.
        /// </summary>
        /// <param name="sender">The workspace whose minimized items changed.</param>
        /// <param name="eventArgs">Information about the change.</param>
        private void ForwardMinimizedItemsChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs) => this.minimizedItemsChanged?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemClosedEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemClosed(Object? sender, DockItemClosedEventArgs eventArgs)
        {
            this.ItemClosed?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Forwards <see cref="DockItemClosingEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemClosing(Object? sender, DockItemClosingEventArgs eventArgs) => this.ItemClosing?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemHiddenEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemHidden(Object? sender, DockItemHiddenEventArgs eventArgs)
        {
            this.ItemHidden?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Forwards <see cref="DockItemHidingEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemHiding(Object? sender, DockItemHidingEventArgs eventArgs) => this.ItemHiding?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemMinimizedEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemMinimized(Object? sender, DockItemMinimizedEventArgs eventArgs)
        {
            this.ItemMinimized?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Forwards <see cref="DockItemMinimizingEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemMinimizing(Object? sender, DockItemMinimizingEventArgs eventArgs) => this.ItemMinimizing?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemMovedEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemMoved(Object? sender, DockItemMovedEventArgs eventArgs)
        {
            this.ItemMoved?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Forwards <see cref="DockItemMovingEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemMoving(Object? sender, DockItemMovingEventArgs eventArgs) => this.ItemMoving?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemRestoredEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemRestored(Object? sender, DockItemRestoredEventArgs eventArgs)
        {
            this.ItemRestored?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Forwards <see cref="DockItemRestoringEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemRestoring(Object? sender, DockItemRestoringEventArgs eventArgs) => this.ItemRestoring?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemShowingEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemShowing(Object? sender, DockItemShowingEventArgs eventArgs) => this.ItemShowing?.Invoke(sender, eventArgs);

        /// <summary>
        /// Forwards <see cref="DockItemShownEventArgs"/> events from all workspaces to event subscribers.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that raised the event.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the event request.</param>
        private void ForwardItemShown(Object? sender, DockItemShownEventArgs eventArgs)
        {
            this.ItemShown?.Invoke(sender, eventArgs);
            this.OnLayoutChanged(new LayoutChangedEventArgs());
        }

        /// <summary>
        /// Handles a request from a <see cref="DockItemViewModel"/> to be floated in the UI.
        /// </summary>
        /// <param name="sender">The <see cref="Object"/> that requested to be minimized.</param>
        /// <param name="eventArgs">The event arguments containing additional information about the float request.</param>
        private void HandleItemFloatRequested(Object? sender, DockItemFloatRequestedEventArgs eventArgs) => this.FloatItem(eventArgs.Item, eventArgs.ScreenLocation, eventArgs.WindowSize);

        /// <summary>
        /// Handles hooking a single <see cref="DockItemViewModel"/> so the current <see cref="DockControlManager"/>
        /// will be notified of changes it needs to update state based on.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to proces.</param>
        private void HookItem(DockItemViewModel item)
        {
            item.FloatRequested += this.HandleItemFloatRequested;
        }

        /// <summary>
        /// Handles removing all handlers for a <see cref="DockItemViewModel"/> so the current <see cref="DockWorkspaceManager"/>
        /// will no longer be notified of changes.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> to process.</param>
        private void UnhookItem(DockItemViewModel item)
        {
            item.FloatRequested -= this.HandleItemFloatRequested;
        }
    }
}
