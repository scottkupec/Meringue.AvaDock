// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Monitors a <see cref="DockNodeViewModel"/> tree for <see cref="DockItemViewModel"/> instances
    /// or <see cref="DockNodeViewModel"/> instances being added or removed. Provides callbacks for hooking and unhooking individual items or nodes.
    /// <para>
    /// This class handles all traversal and subscription to child node collections so that
    /// consumers do not need to duplicate the boilerplate monitoring logic.
    /// </para>
    /// </summary>
    internal sealed class DockNodeMonitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockNodeMonitor"/> class for monitoring item changes.
        /// </summary>
        /// <param name="hookItemCallback">
        /// Action invoked for every <see cref="DockItemViewModel"/> encountered in the monitored tree.
        /// Called once for each existing item and again whenever an item is added.
        /// </param>
        /// <param name="unhookItemCallback">
        /// Action invoked when a <see cref="DockItemViewModel"/> is removed from the monitored tree.
        /// </param>
        public DockNodeMonitor(Action<DockItemViewModel>? hookItemCallback, Action<DockItemViewModel>? unhookItemCallback)
            : this(hookItemCallback, unhookItemCallback, null, null)
        {
            this.HookItemCallback = hookItemCallback;
            this.UnhookItemCallback = unhookItemCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockNodeMonitor"/> class for monitoring node changes.
        /// </summary>
        /// <param name="hookNodeCallback">
        /// Action invoked for every <see cref="DockNodeViewModel"/> encountered in the monitored tree.
        /// Called once for each existing node and again whenever a node is added.
        /// </param>
        /// <param name="unhookNodeCallback">
        /// Action invoked when a <see cref="DockNodeViewModel"/> is removed from the monitored tree.
        /// </param>
        public DockNodeMonitor(Action<DockNodeViewModel>? hookNodeCallback, Action<DockNodeViewModel>? unhookNodeCallback)
            : this(null, null, hookNodeCallback, unhookNodeCallback)
        {
            this.HookNodeCallback = hookNodeCallback;
            this.UnhookNodeCallback = unhookNodeCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockNodeMonitor"/> class for monitoring both item and node changes.
        /// </summary>
        /// <param name="hookItemCallback">
        /// Action invoked for every <see cref="DockItemViewModel"/> encountered in the monitored tree.
        /// Called once for each existing item and again whenever an item is added.
        /// </param>
        /// <param name="unhookItemCallback">
        /// Action invoked when a <see cref="DockItemViewModel"/> is removed from the monitored tree.
        /// </param>
        /// <param name="hookNodeCallback">
        /// Action invoked for every <see cref="DockNodeViewModel"/> encountered in the monitored tree.
        /// Called once for each existing node and again whenever a node is added.
        /// </param>
        /// <param name="unhookNodeCallback">
        /// Action invoked when a <see cref="DockNodeViewModel"/> is removed from the monitored tree.
        /// </param>
        public DockNodeMonitor(
            Action<DockItemViewModel>? hookItemCallback,
            Action<DockItemViewModel>? unhookItemCallback,
            Action<DockNodeViewModel>? hookNodeCallback,
            Action<DockNodeViewModel>? unhookNodeCallback)
        {
            this.HookItemCallback = hookItemCallback;
            this.UnhookItemCallback = unhookItemCallback;
            this.HookNodeCallback = hookNodeCallback;
            this.UnhookNodeCallback = unhookNodeCallback;
        }

        /// <summary>Gets the action to be called when a <see cref="DockItemViewModel"/> is added to the tree.</summary>
        private Action<DockItemViewModel>? HookItemCallback { get; }

        /// <summary>Gets the action to be called when a <see cref="DockNodeViewModel"/> is added to the tree.</summary>
        private Action<DockNodeViewModel>? HookNodeCallback { get; }

        /// <summary>Gets the collection of <see cref="DockNodeViewModel"/>s currently being monitored.</summary>
        private HashSet<DockNodeViewModel> MonitoredRoots { get; } = [];

        /// <summary>Gets the action to be called when a <see cref="DockItemViewModel"/> is removed from the tree.</summary>
        private Action<DockItemViewModel>? UnhookItemCallback { get; }

        /// <summary>Gets the action to be called when a <see cref="DockNodeViewModel"/> is removed from the tree.</summary>
        private Action<DockNodeViewModel>? UnhookNodeCallback { get; }

        /// <summary>
        /// Begins monitoring a <see cref="DockNodeViewModel"/> tree, applying hooks
        /// to all existing and future <see cref="DockItemViewModel"/> instances.
        /// </summary>
        /// <param name="node">The root node to monitor.</param>
        public void Monitor(DockNodeViewModel node)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(node);

            if (this.MonitoredRoots.Add(node))
            {
                this.HookNode(node);
            }
        }

        /// <summary>
        /// Stops monitoring the specified <see cref="DockNodeViewModel"/> root and its descendants.
        /// </summary>
        /// <param name="root">The root node to unmonitor.</param>
        public void Unmonitor(DockNodeViewModel root)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(root);

            if (this.MonitoredRoots.Remove(root))
            {
                this.UnhookNode(root);
            }
        }

        /// <summary>
        /// Attaches subscriptions to a <see cref="DockNodeViewModel"/>.
        /// </summary>
        /// <param name="node">The node to hook.</param>
        private void HookNode(DockNodeViewModel node)
        {
            this.HookNodeCallback?.Invoke(node);

            if (node is DockTabNodeViewModel tabNode)
            {
                tabNode.ObservableTabs.CollectionChanged += this.OnTabNodeCollectionChanged;

                foreach (DockItemViewModel item in tabNode.Tabs)
                {
                    this.HookItemCallback?.Invoke(item);
                }
            }
            else if (node is DockSplitNodeViewModel splitNode)
            {
                splitNode.ChildrenChanged += this.OnSplitNodeChildrenChanged;

                foreach (DockNodeViewModel child in splitNode.Children)
                {
                    this.HookNode(child);
                }
            }
        }

        /// <summary>
        /// Handles collection changes in a <see cref="DockSplitNodeViewModel"/>.
        /// </summary>
        /// <param name="sender">The source of the event (a tab collection).</param>
        /// <param name="eventArgs">The event data describing the collection change.</param>
        private void OnSplitNodeChildrenChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.NewItems != null)
            {
                foreach (DockNodeViewModel newChild in eventArgs.NewItems)
                {
                    this.HookNode(newChild);
                }
            }

            if (eventArgs.OldItems != null)
            {
                foreach (DockNodeViewModel oldChild in eventArgs.OldItems)
                {
                    this.UnhookNode(oldChild);
                }
            }
        }

        /// <summary>
        /// Handles collection changes in a <see cref="DockTabNodeViewModel"/>.
        /// </summary>
        /// <param name="sender">The source of the event (a tab collection).</param>
        /// <param name="eventArgs">The event data describing the collection change.</param>
        private void OnTabNodeCollectionChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.NewItems != null)
            {
                foreach (DockItemViewModel item in eventArgs.NewItems)
                {
                    this.HookItemCallback?.Invoke(item);
                }
            }

            if (eventArgs.OldItems != null)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems)
                {
                    this.UnhookItemCallback?.Invoke(item);
                }
            }
        }

        /// <summary>
        /// Detaches subscriptions from a <see cref="DockNodeViewModel"/>.
        /// </summary>
        /// <param name="node">The node to unhook.</param>
        private void UnhookNode(DockNodeViewModel node)
        {
            if (node is DockTabNodeViewModel tabNode)
            {
                tabNode.ObservableTabs.CollectionChanged -= this.OnTabNodeCollectionChanged;

                foreach (DockItemViewModel item in tabNode.Tabs)
                {
                    this.UnhookItemCallback?.Invoke(item);
                }
            }
            else if (node is DockSplitNodeViewModel splitNode)
            {
                splitNode.ChildrenChanged -= this.OnSplitNodeChildrenChanged;

                foreach (DockNodeViewModel child in splitNode.Children)
                {
                    this.UnhookNode(child);
                }
            }

            this.UnhookNodeCallback?.Invoke(node);
        }
    }
}
