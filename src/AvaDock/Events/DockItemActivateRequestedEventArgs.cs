// Copyright (C) Scott Kupec. All rights reserved.

using System.ComponentModel;
using Meringue.AvaDock.ViewModels;

namespace Meringue.AvaDock.Events
{
    /// <summary>
    /// Provides data for the <see cref="DockItemViewModel.MakeActiveCommand"/> event.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DockItemActivateRequestedEventArgs : DockItemRequestEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockItemActivateRequestedEventArgs"/> class.
        /// </summary>
        /// <param name="item">The <see cref="DockItemViewModel"/> that is requesting to be activated.</param>
        public DockItemActivateRequestedEventArgs(DockItemViewModel item)
            : base(item)
        {
        }
    }
}
