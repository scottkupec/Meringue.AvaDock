// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Monitors a <see cref="DockNodeViewModel"/> tree for <see cref="DockItemViewModel"/> instances
    /// being added or removed. Provides callbacks for hooking and unhooking individual items.
    /// <para>
    /// This class handles all traversal and subscription to child node collections so that
    /// consumers do not need to duplicate the boilerplate monitoring logic.
    /// </para>
    /// </summary>
    public sealed class DockNodeMonitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockNodeMonitor"/> class.
        /// </summary>
        /// <param name="hookCallback">
        /// Action invoked for every <see cref="DockItemViewModel"/> encountered in the monitored tree.
        /// Called once for each existing item and again whenever an item is added.
        /// </param>
        /// <param name="unhookCallback">
        /// Action invoked when a <see cref="DockItemViewModel"/> is removed from the monitored tree.
        /// </param>
        public DockNodeMonitor(Action<DockItemViewModel> hookCallback, Action<DockItemViewModel> unhookCallback)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(hookCallback);
            TargetFrameworkHelper.ThrowIfArgumentNull(unhookCallback);

            this.HookCallback = hookCallback;
            this.UnhookCallback = unhookCallback;
        }

        /// <summary>Gets the action to be called when a <see cref="DockItemViewModel"/> is added to the tree.</summary>
        private Action<DockItemViewModel> HookCallback { get; }

        /// <summary>Gets the collection of <see cref="DockNodeViewModel"/>s currently being monitored.</summary>
        private HashSet<DockNodeViewModel> MonitoredRoots { get; } = [];

        /// <summary>Gets the action to be called when a <see cref="DockItemViewModel"/> is removed from the tree.</summary>
        private Action<DockItemViewModel> UnhookCallback { get; }

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
            if (node is DockTabNodeViewModel tabNode)
            {
                tabNode.ObservableTabs.CollectionChanged += this.OnTabNodeCollectionChanged;

                foreach (DockItemViewModel item in tabNode.Tabs)
                {
                    this.HookCallback(item);
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
                    this.HookCallback(item);
                }
            }

            if (eventArgs.OldItems != null)
            {
                foreach (DockItemViewModel item in eventArgs.OldItems)
                {
                    this.UnhookCallback(item);
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
                    this.UnhookCallback(item);
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
        }
    }
}
