// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Avalonia;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.FloatRequested"/> event.
    /// </summary>
    /// <remarks>
    /// There are no associated Floating/Floated events because those are normal moves.  This event is used to bind
    /// UI actions to the create-workspace + move-item sequence.  Listen for:
    /// <list type="bullet">
    ///   <item><see cref="DockWorkspaceAttachingEventArgs"/></item>
    ///   <item><see cref="DockWorkspaceAttachedEventArgs"/></item>
    ///   <item><see cref="DockItemMovingEventArgs"/></item>
    ///   <item><see cref="DockItemMovedEventArgs"/></item>
    ///  </list>
    ///  to get the events associated with a float action.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemFloatRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemFloatRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be floated.</param>
        /// <param name="screenLocation">The requested top-left position of the floated window in screen coordinates. Optional.</param>
        /// <param name="windowSize">The requested size of the floating window. Optional.</param>
        public DockItemFloatRequestedEventArgs(DockItemViewModel item, PixelPoint? screenLocation, Size? windowSize)
            : base(item)
        {
            this.ScreenLocation = screenLocation;
            this.WindowSize = windowSize;
        }

        /// <summary>
        /// Gets the requested top-left position of the floated window in screen coordinates, if provided.
        /// </summary>
        public PixelPoint? ScreenLocation { get; }

        /// <summary>
        /// Gets the requested size of the floating window, if provided.
        /// </summary>
        public Size? WindowSize { get; }
    }
}
