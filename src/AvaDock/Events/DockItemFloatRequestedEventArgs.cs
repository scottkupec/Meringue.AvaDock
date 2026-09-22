// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Avalonia;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.FloatRequested"/> event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class DockItemFloatRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemFloatRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be floated.</param>
        /// <param name="screenLocation">The top-left position of the floated window in screen coordinates. Optional.</param>
        /// <param name="windowSize">The size of the floating window. Optional.</param>
        public DockItemFloatRequestedEventArgs(DockItemViewModel item, PixelPoint? screenLocation, Size? windowSize)
            : base(item)
        {
            this.ScreenLocation = screenLocation;
            this.WindowSize = windowSize;
        }

        /// <summary>
        /// Gets the top-left position of the floated window in screen coordinates.
        /// </summary>
        public PixelPoint? ScreenLocation { get; }

        /// <summary>
        /// Gets the size of the floating window.
        /// </summary>
        public Size? WindowSize { get; }
    }
}
