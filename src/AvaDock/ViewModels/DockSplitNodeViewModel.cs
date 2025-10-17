// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Layout;
using Meringue.AvaDock.Controls;

namespace Meringue.AvaDock.ViewModels
{
    /// <summary>
    /// Defines the view model for use with <see cref="DockSplitPanel"/>.
    /// </summary>
    public partial class DockSplitNodeViewModel : DockNodeViewModel
    {
        /// <summary>Backing field for the <see cref="Children"/> property.</summary>
        private readonly ObservableCollection<DockNodeViewModel> children = [];

        /// <summary>Backing field for the <see cref="Sizes"/> property.</summary>
        private readonly ObservableCollection<Double> sizes = [];

        /// <summary>
        /// Used to signal the control when changes are ready to be rebuilt in the UI in order to
        /// avoid race conditions of multiple changes occuring in quick succession.
        /// </summary>
        private Boolean needsRebuilt;

        /// <summary>
        /// Gets the <see cref="Avalonia.Layout.Orientation"/> used when multiple children are present.
        /// </summary>
        /// <remarks>
        /// The orientation is the reverse of the splitter direction. Horizontal orientation
        /// uses vertical splitters and veritical orientation uses horizontal splitters.
        /// </remarks>
        private Orientation orientation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockSplitNodeViewModel"/> class.
        /// </summary>
        /// <param name="orientation">The <see cref="Orientation"/> used when multiple children are present.</param>
        public DockSplitNodeViewModel(Orientation orientation)
        {
            this.Orientation = orientation;
            this.Children = new ReadOnlyObservableCollection<DockNodeViewModel>(this.children);
            this.Sizes = new ReadOnlyObservableCollection<Double>(this.sizes);
        }

        /// <summary>
        /// Occurs when the contents of the <see cref="Children"/> collection change.
        /// </summary>
        /// <remarks>
        /// The event is raised whenever items are added to, removed from, or replaced
        /// within the <see cref="Children"/> collection. It mirrors the behavior of
        /// the <see cref="ObservableCollection{T}.CollectionChanged"/> event on the
        /// internal backing collection.
        /// </remarks>
        public event NotifyCollectionChangedEventHandler? ChildrenChanged;

        /// <summary>
        /// Gets the size currently used for each <see cref="DockNodeViewModel"/> in <see cref="children"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Bound property.")]
        public ReadOnlyObservableCollection<DockNodeViewModel> Children { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the control should rebuild the UI. Used to avoid race conditions of multiple
        /// changes occurring in quick succession.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Used to signal the control to rebuild visuals. Not intended for direct use.
        public Boolean NeedsRebuilt
        {
            get => this.needsRebuilt;
            set => this.SetProperty(ref this.needsRebuilt, value);
        }

        /// <summary>
        /// Gets the <see cref="Avalonia.Layout.Orientation"/> used when multiple children are present.
        /// </summary>
        /// <remarks>
        /// The orientation is the reverse of the splitter direction. Horizontal orientation
        /// uses vertical splitters and veritical orientation uses horizontal splitters.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)] // Sync'ed with the control for serialization. Not intended for direct use.
        public Orientation Orientation
        {
            get => this.orientation;
            private set => this.SetProperty(ref this.orientation, value);
        }

        /// <summary>
        /// Gets the size currently used for each child in <see cref="children"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // Sync'ed with the control for serialization. Not intended for direct use.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Bound property.")]
        public ReadOnlyObservableCollection<Double> Sizes { get; }

        /// <summary>Adds a new child to the current control.</summary>
        /// <param name="child">The <see cref="DockNodeViewModel"/> to add as a child.</param>
        /// <param name="size">The proportional size for the new child.</param>
        public void AddChild(DockNodeViewModel child, Double size = 1.0)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(child);

            this.children.Add(child);
            this.sizes.Add(size);

            this.ChildrenChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    child,
                    this.children.Count - 1));
        }

        /// <summary>
        /// Signals that a changes to the split node have been made and that the associated <see cref="DockSplitPanel"/> should rebuild its layout.
        /// </summary>
        /// <remarks>
        /// This method sets <see cref="NeedsRebuilt"/> to <c>true</c>, which is observed by the control and triggers a visual update.
        /// It should be called by layout managers or mutation orchestrators after completing a series of insertions, removals, or
        /// replacements to ensure the UI reflects the updated logical structure. It avoids premature or repeated layout rebuilds during
        /// intermediate mutation steps.
        /// </remarks>
        public void CommitChanges()
        {
            foreach (DockNodeViewModel child in this.Children)
            {
                (child as DockSplitNodeViewModel)?.CommitChanges();
            }

            this.NeedsRebuilt = true;
        }

        /// <summary>
        /// Gets the child <see cref="DockNodeViewModel"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the <see cref="DockNodeViewModel"/> to get.</param>
        /// <returns>The <see cref="DockNodeViewModel"/> at the specified index.</returns>
        public DockNodeViewModel GetChildAt(Int32 index)
        {
            return index < 0 || index >= this.Children.Count
                ? throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {this.Children.Count}")
                : this.Children[index];
        }

        /// <summary>
        /// Gets the index of the specified child node.
        /// </summary>
        /// <param name="child">The <see cref="DockNodeViewModel"/> to return the index of.</param>
        /// <returns>
        /// The zero-based index of the <see cref="DockNodeViewModel"/>, if found; otherwise, -1.
        /// </returns>
        public Int32 IndexOf(DockNodeViewModel child)
        {
            return this.Children.IndexOf(child);
        }

        /// <summary>
        /// Inserts a <see cref="DockNodeViewModel"/> into the layout at the specified index,
        /// along with a corresponding size value.
        /// </summary>
        /// <param name="index">The zero-based index at which the child is to be inserted.</param>
        /// <param name="child">The <see cref="DockNodeViewModel"/> to insert.</param>
        /// <param name="size">
        /// The initial size allocation for the child. Must be positive; values less than or equal to zero
        /// will be clamped to a minimum threshold.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="child"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="index"/> is less than zero or greater than the number of children.
        /// </exception>
        public void InsertAt(Int32 index, DockNodeViewModel child, Double size = 1.0)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(child);

            if (index < 0 || index > this.children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {this.children.Count}");
            }

            this.children.Insert(index, child);
            this.sizes.Insert(index, Math.Max(size, 0.1));

            this.ChildrenChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    child,
                    index));
        }

        /// <summary>Removes a child from the current control.</summary>
        /// <param name="child">The child to be removed.</param>
        public void RemoveChild(DockNodeViewModel child)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(child);

            Int32 index = this.children.IndexOf(child);
            if (index >= 0)
            {
                this.RemoveChildAt(index);
            }
        }

        /// <summary>Removes a child from the current control.</summary>
        /// <param name="index">The index of the child to be removed.</param>
        public void RemoveChildAt(Int32 index)
        {
            if (index < 0 || index > this.children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {this.children.Count}");
            }

            DockNodeViewModel removed = this.children[index];

            this.sizes.RemoveAt(index);
            this.children.RemoveAt(index);

            this.ChildrenChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removed,
                    index));
        }

        /// <summary>
        /// Recursively walks the current instance and removes any empty child nodes including
        /// nodes with a single child by moving the child into the node's location.
        /// </summary>
        public void RemoveEmptyPanels()
        {
            DockSplitNodeViewModel.RemoveEmptyPanelsRecursive(this);
        }

        /// <summary>Replaces the <see cref="DockNodeViewModel"/> at the specified <paramref name="index"/> with the node <paramref name="newChild"/>.</summary>
        /// <param name="index">The zero-based index of the child to be replaced.</param>
        /// <param name="newChild">The <see cref="DockNodeViewModel"/> to place at the index.</param>
        public void ReplaceChildAt(Int32 index, DockNodeViewModel newChild)
        {
            if (index < 0 || index > this.children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {this.children.Count}");
            }

            Double size = this.sizes[index];
            this.RemoveChildAt(index);
            this.InsertAt(index, newChild, size);
        }

        /// <summary>Updates <see cref="Sizes"/> based on control changes.</summary>
        /// <param name="newSizes">The new values for <see cref="Sizes"/>.</param>
        public void UpdateSizes(IEnumerable<Double> newSizes)
        {
            TargetFrameworkHelper.ThrowIfArgumentNull(newSizes);
            Int32 newSizesCount = newSizes.Count();

            if (newSizesCount != this.children.Count)
            {
                throw new ArgumentException(
                    $"Expected {this.children.Count} sizes but got {newSizesCount} sizes",
                    nameof(newSizes));
            }

            this.sizes.Clear();
            foreach (Double size in newSizes)
            {
                this.sizes.Add(Math.Max(size, 0.1));
            }
        }

        /// <summary>
        /// Recursively walks the docking tree starting at <paramref name="parent"/> and
        /// removes any empty child nodes.
        /// </summary>
        /// <param name="parent">The parent split node to inspect.</param>
        private static void RemoveEmptyPanelsRecursive(DockSplitNodeViewModel parent)
        {
            // Copy to list so we can safely modify during iteration
            foreach (DockNodeViewModel child in parent.Children.ToList())
            {
                switch (child)
                {
                    case DockSplitNodeViewModel splitChild:
                        RemoveEmptyPanelsRecursive(splitChild);

                        // After recursion, remove if it has no children
                        if (!splitChild.Children.Any())
                        {
                            parent.RemoveChild(splitChild);
                        }
                        else if (splitChild.Children.Count == 1)
                        {
                            DockNodeViewModel promoted = splitChild.Children[0];
                            Int32 index = parent.Children.IndexOf(splitChild);
                            parent.RemoveChild(splitChild);
                            parent.InsertAt(index, promoted);
                        }
                        else if (splitChild is DockSplitNodeViewModel nested && nested.Orientation == parent.Orientation)
                        {
                            Int32 index = parent.Children.IndexOf(splitChild);
                            parent.RemoveChild(splitChild);

                            foreach (DockNodeViewModel grandchild in nested.Children)
                            {
                                parent.InsertAt(index++, grandchild);
                            }
                        }

                        break;

                    case DockTabNodeViewModel tabChild:
                        // Remove if no tabs
                        if (!tabChild.Tabs.Any())
                        {
                            parent.RemoveChild(tabChild);
                        }

                        break;

                    default:
                        // Defensive — future node types
                        throw new InvalidOperationException(
                            $"Unexpected DockNodeViewModel type: {child.GetType()}, id: {child.Id}");
                }
            }
        }
    }
}
