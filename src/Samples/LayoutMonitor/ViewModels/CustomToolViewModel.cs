// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace LayoutMonitor.ViewModels
{
    /// <summary>
    /// A simple custom tool view model that can be used with the docking system.
    /// </summary>
    public partial class CustomToolViewModel : DockItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomToolViewModel"/> class.
        /// </summary>
        public CustomToolViewModel()
        {
            this.Title = "Custom Tool";
        }
    }
}
