// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Provides an attached behavior that tracks pointer hover state for controls
    /// and updates the <c>IsHovered</c> property on their <see cref="StyledElement.DataContext"/>.
    /// Intended for use with <see cref="DockItemViewModel"/> instances in templates
    /// where direct event wiring is not supported.
    /// </summary>
    internal static class HoverBehavior
    {
        /// <summary>
        /// Identifies the <see cref="EnableHoverTrackingProperty"/> attached property.
        /// When set to <c>true</c>, the control will listen for pointer events and
        /// update <c>IsHovered</c> on its <see cref="StyledElement.DataContext"/> if applicable.
        /// </summary>
        public static readonly AttachedProperty<Boolean> EnableHoverTrackingProperty =
            AvaloniaProperty.RegisterAttached<Control, Control, Boolean>("EnableHoverTracking");

        /// <summary>
        /// An attached property that stores a <see cref="CancellationTokenSource"/> used to manage delayed hover activation per control.
        /// </summary>
        /// <remarks>
        /// This token is created when a pointer enters a control and is used to delay setting
        /// <c>HoveredItem</c> until the user has hovered for a short duration. It is canceled if the pointer exits
        /// before the delay completes. This property ensures that hover state is tracked independently per control.
        /// </remarks>
        public static readonly AttachedProperty<CancellationTokenSource?> HoverTokenProperty =
            AvaloniaProperty.RegisterAttached<Control, CancellationTokenSource?>("HoverToken", typeof(HoverBehavior));

        /// <summary>
        /// Initializes static members of the <see cref="HoverBehavior"/> class.
        /// Subscribes to changes on <see cref="EnableHoverTrackingProperty"/>.
        /// </summary>
        static HoverBehavior()
        {
            _ = EnableHoverTrackingProperty.Changed.Subscribe(OnEnableHoverTrackingChanged);
        }

        /// <summary>
        /// Gets the value of the <see cref="EnableHoverTrackingProperty"/> attached property.
        /// </summary>
        /// <param name="element">The control to read the property from.</param>
        /// <returns><c>true</c> if hover tracking is enabled; otherwise, <c>false</c>.</returns>
        public static Boolean GetEnableHoverTracking(AvaloniaObject element) =>
            element.GetValue(EnableHoverTrackingProperty);

        /// <summary>
        /// Gets the hover delay token associated with the specified control.
        /// </summary>
        /// <param name="control">The control whose hover token is being retrieved.</param>
        /// <returns>
        /// The <see cref="CancellationTokenSource"/> associated with the control, or <c>null</c> if none is set.
        /// </returns>
        public static CancellationTokenSource? GetHoverToken(Control control) =>
            control.GetValue(HoverTokenProperty);

        /// <summary>
        /// Sets the value of the <see cref="EnableHoverTrackingProperty"/> attached property.
        /// </summary>
        /// <param name="element">The control to set the property on.</param>
        /// <param name="value"><c>true</c> to enable hover tracking; otherwise, <c>false</c>.</param>
        public static void SetEnableHoverTracking(AvaloniaObject element, Boolean value) =>
            element.SetValue(EnableHoverTrackingProperty, value);

        /// <summary>
        /// Sets the hover delay token for the specified control.
        /// </summary>
        /// <param name="control">The control to associate with the hover token.</param>
        /// <param name="value">The <see cref="CancellationTokenSource"/> to assign.</param>
        public static void SetHoverToken(Control control, CancellationTokenSource? value) =>
            control.SetValue(HoverTokenProperty, value);

        /// <summary>
        /// Called when <see cref="EnableHoverTrackingProperty"/> changes.
        /// Attaches or detaches pointer event handlers based on the new value.
        /// </summary>
        /// <param name="args">The property change event arguments.</param>
        private static void OnEnableHoverTrackingChanged(AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Sender is Control control)
            {
                control.PointerEntered -= OnPointerEnteredAsync;
                control.PointerExited -= OnPointerExited;

                if (args.NewValue is true)
                {
                    control.PointerEntered += OnPointerEnteredAsync;
                    control.PointerExited += OnPointerExited;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="InputElement.PointerEntered"/> event.
        /// Sets <c>IsHovered</c> to <c>true</c> on the control's <see cref="StyledElement.DataContext"/>
        /// if it is a <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="eventArgs">The pointer event arguments.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposal owned by OnPointerExited.")]
        private static async void OnPointerEnteredAsync(Object? sender, PointerEventArgs eventArgs)
        {
            if (sender is Control control && control.DataContext is DockItemViewModel item)
            {
                CancellationTokenSource? oldTokenSource = GetHoverToken(control);
#if NET6_0_OR_GREATER
                if (oldTokenSource is not null)
                {
                    await oldTokenSource.CancelAsync().ConfigureAwait(true);
                }
#else
                oldTokenSource?.Cancel();
#endif
                CancellationTokenSource tokenSource = new();
                SetHoverToken(control, tokenSource);

                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(400), tokenSource.Token).ConfigureAwait(false);

                    if (!tokenSource.Token.IsCancellationRequested)
                    {
                        if (DockContext.GetWorkspace(item) is DockWorkspaceManager workspace)
                        {
                            await Dispatcher.UIThread.InvokeAsync(
                                () =>
                                {
                                    if (workspace.MinimizedItems.Contains(item))
                                    {
                                        workspace.HoveredItem = item;
                                    }
                                });
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Hover was interrupted — no action needed
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="InputElement.PointerExited"/> event.
        /// Sets <c>IsHovered</c> to <c>false</c> on the control's <see cref="StyledElement.DataContext"/>
        /// if it is a <see cref="DockItemViewModel"/>.
        /// </summary>
        /// <param name="sender">The control that raised the event.</param>
        /// <param name="eventArgs">The pointer event arguments.</param>
        private static void OnPointerExited(Object? sender, PointerEventArgs eventArgs)
        {
            if (sender is Control control && control.DataContext is DockItemViewModel item)
            {
                CancellationTokenSource? tokenSource = GetHoverToken(control);

                if (tokenSource is not null)
                {
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }

                SetHoverToken(control, null);

                DockWorkspaceManager? workspace = DockContext.GetWorkspace(item);

                if (workspace is not null)
                {
                    workspace.HoveredItem = null;
                }
            }
        }
    }
}
