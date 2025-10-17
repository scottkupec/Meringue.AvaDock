// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Meringue.AvaDock.Controls;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Managers
{
    /// <summary>
    /// Provides a (de)serialization layer for <see cref="DockControlManager"/>.
    /// </summary>
    // "DockLayoutManager<T> : DockLayoutManagerBase" pattern needed to work correctly with AXAML bindings.
    // The additional default DockLayoutManager is just a convenience.
    // CONSIDER: Should this be a supporting service instead of manager?  It was originally concieved as
    //           a control replacement for DockControlManager but has morphed to has-is instead of is-a
    //           and now just seems awkward.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Closely related classes.")]
    public abstract partial class DockLayoutManagerBase : ObservableObject
    {
        /// <summary>
        /// Gets or sets the <see cref="DockControlManager"/> being managed.
        /// </summary>
        [ObservableProperty]
        private DockControlManager dockControl;

        /// <summary>
        /// Gets or sets the <see cref="DockInsertPolicy"/> to be used when adding <see cref="DockItemViewModel"/>s that
        /// are not present in the recorded layout.
        /// </summary>
        // CONSIDER: Move to DockControlManager along with CreateOrUpdateItem.
        [ObservableProperty]
        private DockInsertPolicy insertPolicy = DockInsertPolicy.CreateLast;

        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManagerBase"/> class.
        /// </summary>
        public DockLayoutManagerBase()
            : this(null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManagerBase"/> class.
        /// </summary>
        /// <param name="panels">The names of the top-level split panels to pre-create.</param>
        // CONSIDER: Move to default panel constructor to DockControlManager along with CreateOrUpdateItem.
        public DockLayoutManagerBase(params String[] panels)
        {
            DockSplitNodeViewModel split = new(Orientation.Horizontal);

            if (panels is not null)
            {
                foreach (String panelName in panels)
                {
                    split.AddChild(
                        new DockTabNodeViewModel()
                        {
                            Id = panelName,
                        });
                }
            }

            DockWorkspaceManager workspace = new(split);
            this.dockControl = new DockControlManager(workspace);

            this.Serializer = new JsonDockSerializer();
        }

        /// <summary>
        /// Gets or sets the <see cref="IDockSerializer"/> responsible for loading and saving layouts.
        /// </summary>
        // TODO: Review accessibilty modifiers.
        protected internal IDockSerializer Serializer { get; set; }

        /// <summary>
        /// Applies the provided layout to the <see cref="DockControl"/>.
        /// </summary>
        /// <param name="layout">The <see cref="DockControlData"/> to be applied.</param>
        /// <returns><c>true</c> if the layout was applied; otherwise <c>false</c>.</returns>
        public abstract Boolean ApplyLayout(DockControlData layout);

        /// <summary>
        /// Applies the layout in a <see cref="Stream"/> to the current <see cref="DockControl"/>.
        /// </summary>
        /// <param name="stream">The name of the stream from which the layout should be loaded.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public Boolean LoadLayout(Stream stream)
        {
            DockControlData layout = this.Serializer.Load(stream);
            return this.ApplyLayout(layout);
        }

        /// <summary>
        /// Applies the layout in a file to the current <see cref="DockControl"/>.
        /// </summary>
        /// <param name="filename">The name of the file from which the layout should be loaded.</param>
        /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
        public Boolean LoadLayout(String filename)
        {
            DockControlData layout = this.Serializer.Load(filename);
            return this.ApplyLayout(layout);
        }

        /// <summary>
        /// Saves the layout of the current <see cref="DockControl"/>to a file.
        /// </summary>
        /// <param name="filename">The name of the file that will receive the layout.</param>
        public abstract void SaveLayout(String filename);

        /// <summary>
        /// Saves the layout of the current <see cref="DockControl"/>to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> that will receive the layout.</param>
        public abstract void SaveLayout(Stream stream);
    }

    /// <summary>
    /// Defines a <see cref="DockLayoutManager{T}"/> using the default <see cref="DockItemViewModel"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Closely related classes.")]
    public class DockLayoutManager : DockLayoutManager<DockItemViewModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManager"/> class.
        /// </summary>
        public DockLayoutManager()
            : this(null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManager"/> class.
        /// </summary>
        /// <param name="panels">The names of the top-level split panels to pre-create.</param>
        public DockLayoutManager(params String[] panels)
            : base(panels)
        {
        }
    }

    /// <summary>
    /// Defines the manager for a <see cref="DockControl"/> with serialization helpers.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="DockItemViewModel"/> to use.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Closely related classes.")]
    public partial class DockLayoutManager<T> : DockLayoutManagerBase
        where T : DockItemViewModel, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManager{T}"/> class.
        /// </summary>
        public DockLayoutManager()
            : this(null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockLayoutManager{T}"/> class.
        /// </summary>
        /// <param name="panels">The names of the top-level split panels to pre-create.</param>
        public DockLayoutManager(params String[] panels)
            : base(panels)
        {
        }

        /// <inheritdoc/>
        public override Boolean ApplyLayout(DockControlData layout)
        {
            // CONSIDER: Handle different merge options:
            //       - KeepLoadedContexts (requires serializing contexts first)
            //       - KeepCurrentContexts (current, default)
            //       - RemoveMissingItems (don't re-add items from rootGoingAway)
            //       - UpdateExistingItemsOnly (don't add items from layout that aren't in rootGoingAway)
            //       - KeepAllItems (current, default)

            if (layout is null)
            {
                return false;
            }

            DockControlManager rootBeingBuilt = DockSerializationConverter.BuildViewModel<T>(layout);
            DockControlManager rootGoingAway = this.DockControl;

            foreach (DockItemViewModel runtimeItem in rootGoingAway.Items.ToList())
            {
                DockItemViewModel? serializedItem = rootBeingBuilt.FindItem(runtimeItem.Id);

                runtimeItem.DisableClose = false;
                runtimeItem.CloseCommand.Execute(null);

                if (serializedItem is not null)
                {
                    DockContext.SetDockHost(serializedItem, rootBeingBuilt);
                    serializedItem.Context = runtimeItem.Context;
                }
                else
                {
                    rootBeingBuilt.AddHiddenItem(runtimeItem);
                }
            }

            rootBeingBuilt.PrimaryWorkspace.CommitChanges();
            foreach (DockWorkspaceManager secondaryWorkspace in rootBeingBuilt.SecondaryWorkspaces)
            {
                secondaryWorkspace.CommitChanges();
            }

            this.DockControl = rootBeingBuilt;

            this.OnPropertyChanged(nameof(this.DockControl));
            rootBeingBuilt.ShowAllWindows();

            return true;
        }

        /// <summary>
        /// Adds or updates a <see cref="DockItemViewModel"/> in the layout.
        /// </summary>
        /// <param name="id">The id of the <see cref="DockItemViewModel"/> to be updated.</param>
        /// <param name="title">If not null, the header (title) to set on the created or updated <see cref="DockItemViewModel"/>.</param>
        /// <param name="context">The context to set on the created or updated <see cref="DockItemViewModel"/>.</param>
        /// <param name="defaultParentId">
        /// The optional name of the parent node to use if the <see cref="DockItemViewModel"/> needs to be created.
        /// If no node with the given name exists or if the parameter is null, the behavior of the API is defined by
        /// the <see cref="DockLayoutManagerBase.InsertPolicy"/> property.
        /// </param>
        /// <returns>The <see cref="DockItemViewModel"/> created or updated.</returns>
        // TODO: Task 180 to refactor this to DockControlManager
        public T? CreateOrUpdateItem(String id, String? title, Object context, String? defaultParentId = null)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Item ID cannot be null or whitespace.", nameof(id));
            }

            if (this.DockControl.FindItem(id) is not T item)
            {
                DockNodeViewModel? targetNode = this.DockControl.PrimaryWorkspace.DockTree;

                if (!String.IsNullOrEmpty(defaultParentId))
                {
                    targetNode = this.DockControl.FindNode(defaultParentId!);
                }

                if (targetNode is null)
                {
                    if (this.InsertPolicy == DockInsertPolicy.Error)
                    {
                        throw new ArgumentOutOfRangeException(nameof(defaultParentId), "Parent could not be found");
                    }

                    if (this.InsertPolicy == DockInsertPolicy.CreateFloating)
                    {
                        DockTabNodeViewModel tab = new();
                        DockSplitNodeViewModel split = new(Orientation.Horizontal);
                        split.AddChild(tab);
                        DockWorkspaceManager manager = new(split);

                        IWindow window = this.DockControl.AttachSecondaryWorkspace(
                            manager,
                            null,
                            new Size(300, 200));
                        window.Show();

                        targetNode = tab;
                    }
                    else
                    {
                        targetNode = this.DockControl.PrimaryWorkspace.DockTree;
                    }
                }

                item = new T()
                {
                    Id = id,
                    Title = title ?? String.Empty,
                    Context = context,
                };

                if (targetNode is DockTabNodeViewModel tabNode)
                {
                    tabNode.AddTab(item);
                }
                else if (targetNode is DockSplitNodeViewModel splitNode)
                {
                    DockTabNodeViewModel newTabNode = new();
                    newTabNode.AddTab(item);
                    newTabNode.Selected = item;

                    if (this.InsertPolicy == DockInsertPolicy.CreateFirst)
                    {
                        splitNode.InsertAt(0, newTabNode);
                    }
                    else
                    {
                        splitNode.AddChild(newTabNode);
                    }
                }
                else
                {
                    // Unexpected node type — fallback behavior
                    throw new InvalidOperationException($"Unsupported parent node type: {targetNode.GetType().Name}");
                }
            }
            else
            {
                if (title is not null)
                {
                    item.Title = title;
                }

                item.Context = context;
            }

            DockWorkspaceManager? workspaceUpdated = DockContext.GetWorkspace(item);
            workspaceUpdated?.CommitChanges();

            return item;
        }

        /// <inheritdoc/>
        public override void SaveLayout(String filename)
        {
            DockControlData layout = DockSerializationConverter.BuildLayout<T>(this.DockControl);
            this.Serializer.Save(layout, filename);
        }

        /// <inheritdoc/>
        public override void SaveLayout(Stream stream)
        {
            DockControlData layout = DockSerializationConverter.BuildLayout<T>(this.DockControl);
            this.Serializer.Save(layout, stream);
        }
    }
}
