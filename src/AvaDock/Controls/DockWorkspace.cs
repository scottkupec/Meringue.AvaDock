// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// The top level control for hosting a dock panel and its child controls.
    /// </summary>
    public class DockWorkspace : TemplatedControl
    {
        /// <summary>
        /// Defines the style property for the <see cref="MinimizedItems"/> member.
        /// </summary>
        public static readonly StyledProperty<IList<DockItemViewModel>> MinimizedItemsProperty =
            AvaloniaProperty.Register<DockWorkspace, IList<DockItemViewModel>>(nameof(MinimizedItems));

        /// <summary>
        /// Defines the style property for the <see cref="ShouldShowMinimizedItems"/> member.
        /// </summary>
        public static readonly StyledProperty<Boolean> ShouldShowMinimizedItemsProperty =
            AvaloniaProperty.Register<DockWorkspace, Boolean>(
                nameof(ShouldShowMinimizedItems));

        /// <summary>
        /// Gets or sets the <see cref="DockItemViewModel"/> that are currently minimized.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Bound property")]
        public IList<DockItemViewModel> MinimizedItems
        {
            get => this.GetValue(MinimizedItemsProperty);
            set => this.SetValue(MinimizedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MinimizedItemsStub"/> should be displayed.
        /// </summary>
        public Boolean ShouldShowMinimizedItems
        {
            get => this.GetValue(ShouldShowMinimizedItemsProperty);
            set => this.SetValue(ShouldShowMinimizedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets the stub for managing <see cref="MinimizedItems"/>.
        /// </summary>
        private MinimizedItemsBar? MinimizedItemsStub { get; set; }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (e is not null)
            {
                if (e.NameScope.Find("PART_MinimizedStub") is MinimizedItemsBar stub)
                {
                    this.MinimizedItemsStub = stub;
                    this.MinimizedItemsStub.Items = this.MinimizedItems;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            if (this.DataContext is DockWorkspaceManager viewModel)
            {
                _ = this.Bind(
                    ShouldShowMinimizedItemsProperty,
                    new Binding(nameof(viewModel.ShouldShowMinimizedItems)) { Source = viewModel });

                _ = this.Bind(
                    MinimizedItemsProperty,
                    new Binding(nameof(viewModel.MinimizedItems)) { Source = viewModel });

                if (this.MinimizedItemsStub is not null)
                {
                    this.MinimizedItemsStub.Items = this.MinimizedItems;
                }
            }
        }
    }
}
