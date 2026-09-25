// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// A specialized <see cref="TabControl"/> for use with docking.
    /// </summary>
    public class DockTabPanel : TabControl
    {
        /// <summary>
        /// Defines the style property for the <see cref="ShouldShowTabStrip"/> member.
        /// </summary>
        public static readonly StyledProperty<Boolean> ShouldShowTabStripProperty =
            AvaloniaProperty.Register<DockTabPanel, Boolean>(
                nameof(ShouldShowTabStrip));

        /// <summary>The point where the pointer was initially clicked.</summary>
        private Point? dragStartPoint;

        /// <summary>Indicates for a drag operation is currently in effect.</summary>
        private Boolean isDragging;

        /// <summary>The tab item where the pointer was initially pressed.</summary>
        private TabItem? dragSourceTab;

        /// <summary>The pointer pressed event args captured at drag start.</summary>
        private PointerPressedEventArgs? dragStartEventArgs;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockTabPanel"/> class.
        /// </summary>
        public DockTabPanel()
        {
            this.SubscribeToItemsCollection();

            // Set initial value (in case items exist from XAML or template initialization)
            this.UpdateTabStripVisibility();

            // In case the entire ItemsSource is replaced.
            _ = this.GetObservable(ItemsSourceProperty)
                .Subscribe(_ =>
                {
                    this.SubscribeToItemsCollection();
                    this.UpdateTabStripVisibility();
                });
        }

        /// <summary>
        /// Gets or sets a value indicating whether the corresponding <see cref="TabStrip"/> should
        /// be displayed.
        /// </summary>
        public Boolean ShouldShowTabStrip
        {
            get => this.GetValue(ShouldShowTabStripProperty);
            set => this.SetValue(ShouldShowTabStripProperty, value);
        }

        /// <summary>
        /// Gets or sets the last <see cref="DockTabPanel"/> that the pointer was last over in order to work around
        /// OnDragLeave not always being called.
        /// </summary>
        private static DockTabPanel? LastPanelOver { get; set; }

        /// <summary>
        /// Gets or sets the collection of items currently subscribed to.
        /// </summary>
        private INotifyCollectionChanged? CurrentItemsCollection { get; set; }

        /// <summary>Gets or sets the <see cref="DragContext"/> for a drag operation in progress.</summary>
        private DragContext? DragOperationContext { get; set; }

        /// <inheritdoc/>
        protected override void ClearContainerForItemOverride(Control element)
        {
            base.ClearContainerForItemOverride(element);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (this.DataContext is DockTabNodeViewModel viewModel && viewModel.Selected is not null)
            {
                this.SelectedItem = viewModel.Selected;
            }
        }

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.AddHandler(DragDrop.DragEnterEvent, this.OnDragEnter, handledEventsToo: true);
            this.AddHandler(DragDrop.DragLeaveEvent, this.OnDragLeave, handledEventsToo: true);
            this.AddHandler(DragDrop.DragOverEvent, this.OnDragOver, handledEventsToo: true);
            this.AddHandler(DragDrop.DropEvent, this.OnTabDropped, handledEventsToo: true);
            this.AddHandler(Avalonia.Input.InputElement.PointerMovedEvent, this.OnPanelPointerMovedAsync, handledEventsToo: true);
            this.AddHandler(Avalonia.Input.InputElement.PointerPressedEvent, this.OnPanelPointerPressed, handledEventsToo: true);
            this.AddHandler(Avalonia.Input.InputElement.PointerReleasedEvent, this.OnPanelPointerReleased, handledEventsToo: true);
        }

        /// <inheritdoc/>
        protected override void PrepareContainerForItemOverride(Control element, Object? item, Int32 index)
        {
            base.PrepareContainerForItemOverride(element, item, index);
        }

        /// <summary>
        /// Initiates a drag-and-drop operation for the specified <see cref="DockItemViewModel"/> tab.
        /// This method packages the tab into a <see cref="DataTransfer"/> and begins the drag operation
        /// using Avalonia's <see cref="DragDrop.DoDragDropAsync"/> API.
        /// </summary>
        /// <param name="item">The tab view model being dragged.</param>
        /// <param name="eventArgs">The pointer event that triggered the drag, used to anchor the drag context.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        private static async Task<Boolean> TryStartDragAsync(DockItemViewModel item, PointerPressedEventArgs eventArgs)
        {
            // Create the application specific DataTransferItem.
            DataTransferItem itemData = new();
            itemData.Set(DockContext.DockItemDragFormat, item);

            // Build the drag payload and attach application data.
#pragma warning disable CA2000 // Avalonia disposes the DataTransfer. See remarks on https://github.com/AvaloniaUI/Avalonia/blob/main/src/Avalonia.Base/Input/IDataTransfer.cs
            DataTransfer dataTransfer = new();
            dataTransfer.Add(itemData);
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Start async drag operation
            DragDropEffects result = await DragDrop.DoDragDropAsync(
                eventArgs,
                dataTransfer,
                DragDropEffects.Move).ConfigureAwait(false);

            // Return true if a move actually occurred
            return result.HasFlag(DragDropEffects.Move);
        }

        /// <summary>
        /// Called when starting to drag into a control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> for the event.</param>
        private void OnDragEnter(Object? sender, DragEventArgs eventArgs)
        {
            // HACK: OnDragLeave is not always called when the pointer leaves a control, so we need to manually
            //       clean up the previous panel's adorners.
            DockTabPanel.LastPanelOver?.OnDragLeave(sender, eventArgs);
            DockTabPanel.LastPanelOver = this;

            if (eventArgs.DataTransfer.Formats.Contains(DockContext.DockItemDragFormat))
            {
                this.DragOperationContext ??= new DragContext(this, eventArgs);
                this.DragOperationContext.ShowAdorners();
                eventArgs.Handled = true;
            }
        }

        /// <summary>
        /// Called when no longer dragging a tab over the current control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> for the event.</param>
        private void OnDragLeave(Object? sender, DragEventArgs eventArgs)
        {
            this.DragOperationContext?.RemoveAdorners();
            this.DragOperationContext = null;
        }

        /// <summary>
        /// Called when dragging a tab over the current control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> for the event.</param>
        private void OnDragOver(Object? sender, DragEventArgs eventArgs)
        {
            if (eventArgs.DataTransfer.Formats.Contains(DockContext.DockItemDragFormat))
            {
                Point pointerPosition = eventArgs.GetPosition(this);

                // Only handle the event if the pointer is actually over this panel
                if (this.Bounds.Contains(pointerPosition))
                {
                    this.DragOperationContext?.UpdateVisualFeedback(pointerPosition);
                    eventArgs.DragEffects = DragDropEffects.Move;
                    eventArgs.Handled = true;
                }
            }
        }

        /// <summary>
        /// Called when the item collection has changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="NotifyCollectionChangedEventArgs"/> for the event.</param>
        private void OnItemsCollectionChanged(Object? sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateTabStripVisibility();
        }

        /// <summary>
        /// Called when dropping a tab onto the current control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The <see cref="DragEventArgs"/> for the event.</param>
        private void OnTabDropped(Object? sender, DragEventArgs eventArgs)
        {
            if (this.DragOperationContext?.IsValid is true)
            {
                if (!this.DragOperationContext.TryReorder())
                {
                    this.DragOperationContext.DropPanel();

                    if (this is SelectingItemsControl selecting)
                    {
                        selecting.SelectedItem = this.DragOperationContext.DraggedTab;
                    }
                }
            }

            this.DragOperationContext?.RemoveAdorners();
            this.DragOperationContext = null;
        }

        /// <summary>Processes <see cref="PointerEventArgs"/> events when the pointer is moved over the panel.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        private async void OnPanelPointerMovedAsync(Object? sender, PointerEventArgs eventArgs)
        {
            // Only start drag for the tab that was initially pressed
            if (this.dragSourceTab is null || this.dragStartEventArgs is null)
            {
                return;
            }

            if (this.dragSourceTab.DataContext is not DockItemViewModel dockItem)
            {
                return;
            }

            if (eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed && this.dragStartPoint is not null)
            {
                Point pos = eventArgs.GetPosition(this);
                Point delta = pos - this.dragStartPoint.Value;

                // Only start drag if moved beyond a threshold
                if (!this.isDragging && (Math.Abs(delta.X) > 4 || Math.Abs(delta.Y) > 4))
                {
                    this.isDragging = true;
                    _ = await DockTabPanel.TryStartDragAsync(dockItem, this.dragStartEventArgs).ConfigureAwait(false);
                }
            }
        }

        /// <summary>Processes <see cref="PointerPressedEventArgs"/> events when the pointer is pressed on the panel.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        private void OnPanelPointerPressed(Object? sender, PointerPressedEventArgs eventArgs)
        {
            if (!eventArgs.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                return;
            }

            if (eventArgs.Source is not Visual sourceVisual)
            {
                return;
            }

            TabItem? tabItem = sourceVisual.FindAncestorOfType<TabItem>();
            if (tabItem is null || tabItem.DataContext is not DockItemViewModel)
            {
                return;
            }

            Point pos = eventArgs.GetPosition(this);
            this.dragStartPoint = pos;
            this.isDragging = false;
            this.dragSourceTab = tabItem;
            this.dragStartEventArgs = eventArgs;
        }

        /// <summary>Processes <see cref="PointerReleasedEventArgs"/> events when the pointer is released.</summary>
        /// <param name="sender">The sender of the event args.</param>
        /// <param name="eventArgs">The arguments for the event.</param>
        private void OnPanelPointerReleased(Object? sender, PointerReleasedEventArgs eventArgs)
        {
            // Reset drag state on release
            this.dragStartPoint = null;
            this.isDragging = false;
            this.dragSourceTab = null;
            this.dragStartEventArgs = null;
        }

        /// <summary>
        /// Maintains the event subscription for the items collection.
        /// </summary>
        private void SubscribeToItemsCollection()
        {
            this.CurrentItemsCollection?.CollectionChanged -= this.OnItemsCollectionChanged;

            this.CurrentItemsCollection = this.ItemsSource as INotifyCollectionChanged;
            this.CurrentItemsCollection?.CollectionChanged += this.OnItemsCollectionChanged;
        }

        /// <summary>
        /// Keeps <see cref="DockTabPanel.ShouldShowTabStrip"/> current as items are added or removed
        /// from the collection.
        /// </summary>
        private void UpdateTabStripVisibility()
        {
            this.ShouldShowTabStrip = this.Items.Count >= 2;
        }

        /// <summary>
        /// Represents the full context of a drag-and-drop operation within a <see cref="DockTabPanel"/>.
        /// Combines logical state (dragged tab, target node, drop index) with visual feedback (adorners).
        /// Used to coordinate drag behavior and visuals across the drag lifecycle.
        /// </summary>
        private class DragContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DragContext"/> class using the drag event data.
            /// </summary>
            /// <param name="owner">The parent <see cref="DockTabPanel"/> initiating the drag.</param>
            /// <param name="eventArgs">The drag event arguments containing the drag data and pointer position.</param>
            public DragContext(DockTabPanel owner, DragEventArgs eventArgs)
            {
                this.Owner = owner;

                this.DraggedTab = eventArgs.DataTransfer.TryGetValue(DockContext.DockItemDragFormat);
                this.DropIndex = this.HitTestTabIndex(eventArgs.GetPosition(owner));
                this.TargetNode = DragContext.GetTargetNode(eventArgs);
            }

            /// <summary>
            /// Gets the tab being dragged, if available.
            /// </summary>
            public DockItemViewModel? DraggedTab { get; }

            /// <summary>
            /// Gets the index of the tab under the pointer, if applicable.
            /// </summary>
            public Int32? DropIndex { get; private set; }

            /// <summary>
            /// Gets the target tab node where the dragged tab may be dropped.
            /// </summary>
            public DockTabNodeViewModel? TargetNode { get; }

            /// <summary>
            /// Gets a value indicating whether the drag operation is valid and actionable.
            /// </summary>
            public Boolean IsValid => this.DraggedTab != null && this.TargetNode != null;

            /// <summary>
            /// Gets or sets the overlay used when dropping new items to the panel.
            /// </summary>
            private TabPanelDropAdorner? DropAdorner { get; set; }

            /// <summary>
            /// Gets the parent <see cref="DockTabPanel"/> that initiated the drag operation.
            /// Used to access visual elements, adorners, and hit testing logic during the drag.
            /// </summary>
            private DockTabPanel Owner { get; }

            /// <summary>
            /// Gets or sets the overlay used when dropping to a specific tab position.
            /// </summary>
            private TabReorderAdorner? ReorderAdorner { get; set; }

            /// <summary>
            /// Attempts to drop the dragged tab into the panel using the current drop zone and orientation.
            /// Raises a move request on the dragged tab.
            /// </summary>
            public void DropPanel()
            {
                DropZone? zone = this.DropAdorner?.IsVisible == true ? this.DropAdorner.HoveredZone : null;
                Int32? insertionIndex = null;
                if (this.DropIndex.HasValue && this.TargetNode != null && this.DraggedTab != null && !this.TargetNode.Tabs.Contains(this.DraggedTab))
                {
                    insertionIndex = this.DropIndex.Value;
                }

                this.DraggedTab?.RequestMove(
                    (this.Owner.DataContext as DockTabNodeViewModel)!,
                    this.TargetNode!,
                    DragContext.GetMovePlacement(zone),
                    DragContext.GetRequiredOrientation(zone),
                    insertionIndex);
            }

            /// <summary>
            /// Removes drag adorners from the visual tree and clears their state.
            /// </summary>
            public void RemoveAdorners()
            {
                if (this.DropAdorner is { Parent: Panel dropAdornerParent })
                {
                    _ = dropAdornerParent.Children.Remove(this.DropAdorner);
                }

                if (this.ReorderAdorner is { Parent: Panel adornerParent })
                {
                    _ = adornerParent.Children.Remove(this.ReorderAdorner);
                }

                this.DropAdorner = null;
                this.ReorderAdorner = null;
            }

            /// <summary>
            /// Ensures that drag adorners are created and made visible.
            /// This method is safe to call multiple times and will not duplicate adorners.
            /// </summary>
            public void ShowAdorners()
            {
                this.EnsureAdorners();
                this.DropAdorner?.SetVisible(true);
                this.ReorderAdorner?.SetVisible(true);
            }

            /// <summary>
            /// Attempts to reorder the dragged tab within the target tab node.
            /// </summary>
            /// <returns><c>true</c> if the tab was successfully moved to a new index; otherwise, <c>false</c>.</returns>
            public Boolean TryReorder()
            {
                if (this.DraggedTab is null || this.TargetNode is null || !this.DropIndex.HasValue)
                {
                    return false;
                }

                if (!this.TargetNode.Tabs.Contains(this.DraggedTab))
                {
                    this.DraggedTab.RequestMove(
                        (this.Owner.DataContext as DockTabNodeViewModel)!,
                        this.TargetNode,
                        MovePlacement.On,
                        null,
                        this.DropIndex.Value);

                    return true;
                }

                Int32 oldIndex = this.TargetNode.ObservableTabs.IndexOf(this.DraggedTab);
                if (oldIndex != this.DropIndex.Value)
                {
                    this.TargetNode.ObservableTabs.Move(oldIndex, this.DropIndex.Value);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Updates the visibility and target of drag adorners based on the current pointer position.
            /// Determines whether the pointer is over the tab strip or the panel body.
            /// </summary>
            /// <param name="pointerPosition">The pointer position relative to the <see cref="DockTabPanel"/>.</param>
            public void UpdateVisualFeedback(Point pointerPosition)
            {
                ItemsPresenter? itemsPresenter = this.Owner.GetVisualDescendants().OfType<ItemsPresenter>().FirstOrDefault();
                if (itemsPresenter != null && itemsPresenter.Bounds.Contains(pointerPosition))
                {
                    this.DropAdorner?.SetVisible(false);
                    this.DropAdorner?.ResetZone();
                    this.ReorderAdorner?.SetVisible(true);

                    Int32? hoverIndex = this.HitTestTabIndex(pointerPosition);
                    this.DropIndex = hoverIndex;

                    if (hoverIndex is not null)
                    {
                        this.ReorderAdorner?.UpdateTarget(this.Owner, hoverIndex.Value);
                    }
                    else
                    {
                        this.ReorderAdorner?.SetVisible(false);
                    }
                }
                else
                {
                    this.DropAdorner?.SetVisible(true);
                    this.ReorderAdorner?.SetVisible(false);
                    this.DropIndex = null;

                    Point localPosition = this.DropAdorner != null
                        ? pointerPosition
                        : default;

                    this.DropAdorner?.UpdateTarget(this.Owner, localPosition);
                }
            }

            /// <summary>
            /// Converts a <see cref="DropZone"/> value to the corresponding <see cref="MovePlacement"/> value.
            /// </summary>
            /// <param name="zone">The <see cref="DropZone"/> to be converted.</param>
            /// <returns>The necessary <see cref="MovePlacement"/> based on the specified <paramref name="zone"/>.</returns>
            private static MovePlacement GetMovePlacement(DropZone? zone)
            {
                return zone switch
                {
                    DropZone.Bottom => MovePlacement.After,
                    DropZone.Center => MovePlacement.On,
                    DropZone.Left => MovePlacement.Before,
                    DropZone.None => MovePlacement.After,
                    DropZone.Right => MovePlacement.After,
                    DropZone.Top => MovePlacement.Before,
                    _ => MovePlacement.On,
                };
            }

            /// <summary>
            /// Converts a <see cref="DropZone"/> value to the corresponding <see cref="Orientation"/> value, if applicable.
            /// </summary>
            /// <param name="zone">The <see cref="DropZone"/> to be converted.</param>
            /// <returns>The necessary split <see cref="Orientation"/> based on the specified <paramref name="zone"/> or <c>null</c> if orientation
            /// doesn't apply to the provided <paramref name="zone"/>.</returns>
            private static Orientation? GetRequiredOrientation(DropZone? zone)
            {
                return zone switch
                {
                    DropZone.Bottom => Orientation.Vertical,
                    DropZone.Center => null,
                    DropZone.Left => Orientation.Horizontal,
                    DropZone.None => null,
                    DropZone.Right => Orientation.Horizontal,
                    DropZone.Top => Orientation.Vertical,
                    _ => null,
                };
            }

            /// <summary>
            /// Attempts to locate the target <see cref="DockTabNodeViewModel"/> from the drag event's visual source.
            /// </summary>
            /// <param name="eventArgs">The drag event arguments.</param>
            /// <returns>The target node if found; otherwise, <c>null</c>.</returns>
            private static DockTabNodeViewModel? GetTargetNode(DragEventArgs eventArgs)
            {
                return (eventArgs.Source as Visual)?
                    .GetVisualAncestors()
                    .OfType<Control>()
                    .Select(c => c.DataContext)
                    .OfType<DockTabNodeViewModel>()
                    .FirstOrDefault();
            }

            /// <summary>
            /// Ensures that drag-and-drop adorners are created and added to the visual tree.
            /// This method is safe to call multiple times and will not duplicate adorners.
            /// </summary>
            private void EnsureAdorners()
            {
                AdornerLayer? layer = AdornerLayer.GetAdornerLayer(this.Owner);

                if (layer is not null)
                {
                    if (this.DropAdorner == null)
                    {
                        this.DropAdorner = new TabPanelDropAdorner();
                        layer.Children.Add(this.DropAdorner);
                    }

                    if (this.ReorderAdorner == null)
                    {
                        this.ReorderAdorner = new TabReorderAdorner();
                        layer.Children.Add(this.ReorderAdorner);
                    }
                }
            }

            /// <summary>
            /// Returns the tab index in the tab strip under the pointer, or -1 if none.
            /// </summary>
            /// <param name="pointerPosition">Position relative to the tab strip.</param>
            /// <returns>
            /// The zero-based index of the tab under the pointer, or <c>-1</c> if no tab is under the pointer.
            /// </returns>
            private Int32? HitTestTabIndex(Point pointerPosition)
            {
                if (this.Owner.GetVisualDescendants().FirstOrDefault(v => v is ItemsPresenter) is not ItemsPresenter itemsPresenter)
                {
                    return null;
                }

                for (Int32 i = 0; i < this.Owner.ItemCount; i++)
                {
                    if (this.Owner.ContainerFromIndex(i) is TabItem tabItem)
                    {
                        if (tabItem.Presenter is { } headerControl)
                        {
                            // Bounds relative to TabControl
                            Rect headerRect = headerControl.Bounds;
                            Matrix? transform = headerControl.TransformToVisual(this.Owner);
                            if (transform.HasValue)
                            {
                                Point topLeft = transform.Value.Transform(headerRect.TopLeft);
                                Point bottomRight = transform.Value.Transform(headerRect.BottomRight);
                                headerRect = new Rect(topLeft, bottomRight);
                            }

                            if (headerRect.Contains(pointerPosition))
                            {
                                return i;
                            }
                        }
                    }
                }

                return null; // no tab under pointer
            }
        }
    }
}
