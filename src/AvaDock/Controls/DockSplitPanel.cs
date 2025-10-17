// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// A split panel with resizable regions for use in dock hosts.
    /// </summary>
    [TemplatePart(Name = "PART_Container", Type = typeof(Grid))]
    public partial class DockSplitPanel : TemplatedControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockSplitPanel"/> class.
        /// </summary>
        public DockSplitPanel()
        {
            this.DataContextChanged += (_, _) => this.OnDataContextChanged();
        }

        /// <summary>Gets or sets the grid being presented.</summary>
        private Grid? Container { get; set; }

        /// <summary>
        /// Gets or sets the associated view model for the control.
        /// </summary>
        private DockSplitNodeViewModel? ViewModel { get; set; }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (e is not null)
            {
                this.Container = e.NameScope.Find<Grid>("PART_Container");
                this.RebuildLayout();
            }
        }

        /// <summary>
        /// Helper method for building an enumeration of <see cref="Control"/>s from an enumeration of <see cref="DockNodeViewModel"/>s.
        /// </summary>
        /// <param name="children">The <see cref="DockNodeViewModel"/>s to use for constructing <see cref="Control"/>s.</param>
        /// <returns>The <see cref="Control"/>s built.</returns>
        private static IEnumerable<Control> BuildChildControls(IEnumerable<DockNodeViewModel> children)
        {
            foreach (DockNodeViewModel child in children)
            {
                yield return new DockTree { DataContext = child };
            }
        }

        /// <summary>
        /// Updates the provided <paramref name="container"/> to be a split grid of the <paramref name="children"/>.
        /// </summary>
        /// <param name="container">The <see cref="Grid"/> to be updated.</param>
        /// <param name="orientation">The <see cref="Orientation"/> to use for splitting.</param>
        /// <param name="children">The <see cref="Control"/>s to add to the container.</param>
        private void BuildGrid(Grid? container, Orientation orientation, IEnumerable<Control>? children)
        {
            if (container == null || children == null)
            {
                return;
            }

            container.Children.Clear();
            container.ColumnDefinitions.Clear();
            container.RowDefinitions.Clear();

            Boolean isHorizontal = orientation == Orientation.Horizontal;
            Int32 index = 0;

            foreach (Control child in children)
            {
                if (index > 0)
                {
                    GridSplitter splitter = new()
                    {
                        Width = isHorizontal ? 2 : Double.NaN,
                        Height = isHorizontal ? Double.NaN : 2,
                        MinWidth = isHorizontal ? 2 : 0,
                        MaxWidth = isHorizontal ? 2 : Double.PositiveInfinity,
                        MinHeight = isHorizontal ? 0 : 2,
                        MaxHeight = isHorizontal ? Double.PositiveInfinity : 2,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    splitter.DragCompleted += (_, _) => this.SaveCurrentSizes();

                    if (isHorizontal)
                    {
                        container.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                        Grid.SetColumn(splitter, (index * 2) - 1);
                    }
                    else
                    {
                        container.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                        Grid.SetRow(splitter, (index * 2) - 1);
                    }

                    container.Children.Add(splitter);
                }

                GridLength length = this.ViewModel?.Sizes != null && index < this.ViewModel.Sizes.Count
                    ? new GridLength(this.ViewModel.Sizes[index], GridUnitType.Star)
                    : new GridLength(1.0, GridUnitType.Star);

                if (isHorizontal)
                {
                    container.ColumnDefinitions.Add(new ColumnDefinition(length));
                }
                else
                {
                    container.RowDefinitions.Add(new RowDefinition(length));
                }

                if (isHorizontal)
                {
                    Grid.SetColumn(child, index * 2);
                    Grid.SetRow(child, 0);
                }
                else
                {
                    Grid.SetRow(child, index * 2);
                    Grid.SetColumn(child, 0);
                }

                container.Children.Add(child);

                index++;
            }
        }

        /// <summary>
        /// Called when data context for the view model changes.
        /// </summary>
        private void OnDataContextChanged()
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.PropertyChanged -= this.OnViewModelPropertyChanged;
            }

            if (this.DataContext is DockSplitNodeViewModel viewModel)
            {
                this.ViewModel = viewModel;
                this.ViewModel.PropertyChanged += this.OnViewModelPropertyChanged;
                this.RebuildLayout();
            }
            else
            {
                this.ViewModel = null;
            }
        }

        /// <summary>
        /// Handles property change notifications from the associated <see cref="DockSplitNodeViewModel"/>.
        /// Triggers a layout rebuild when <see cref="DockSplitNodeViewModel.NeedsRebuilt"/> is set to <c>true</c>,
        /// and resets the flag to <c>false</c> after rebuilding.
        /// </summary>
        /// <param name="sender">The source of the property change event.</param>
        /// <param name="eventArgs">The <see cref="PropertyChangedEventArgs"/> containing the name of the changed property.</param>
        private void OnViewModelPropertyChanged(Object? sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == nameof(DockSplitNodeViewModel.NeedsRebuilt) && this.ViewModel?.NeedsRebuilt == true)
            {
                this.RebuildLayout();
            }
        }

        /// <summary>
        /// Rebuilds the control's visuals.
        /// </summary>
        // CONSIDER: Incremental grid rebuild instead of full grid rebuild if we
        //           see any performance issues/visual flickering when this occurs.
        private void RebuildLayout()
        {
            if (this.ViewModel == null || this.Container == null)
            {
                return;
            }

            IEnumerable<Control> controls = BuildChildControls(this.ViewModel.Children);
            this.BuildGrid(this.Container, this.ViewModel.Orientation, controls);
            this.SaveCurrentSizes();
            this.ViewModel.NeedsRebuilt = false;
        }

        /// <summary>Save the current sizes of each column or row.</summary>
        private void SaveCurrentSizes()
        {
            if (this.Container is null || this.ViewModel is null)
            {
                return;
            }

            List<Double> sizes = [];

            if (this.ViewModel.Orientation == Orientation.Horizontal)
            {
                foreach (ColumnDefinition column in this.Container.ColumnDefinitions)
                {
                    if (column.Width.IsStar)
                    {
                        sizes.Add(column.Width.Value);
                    }
                }
            }
            else
            {
                foreach (RowDefinition row in this.Container.RowDefinitions)
                {
                    if (row.Height.IsStar)
                    {
                        sizes.Add(row.Height.Value);
                    }
                }
            }

            System.Diagnostics.Debug.Assert(
                this.ViewModel.Children.Count == sizes.Count,
                $"Number of {nameof(sizes)} should match number of {nameof(this.ViewModel.Children)}.");

            Double total = sizes.Sum();
            if (total > 0)
            {
                this.ViewModel.UpdateSizes(sizes.Select(s => s / total));
            }
        }
    }
}
