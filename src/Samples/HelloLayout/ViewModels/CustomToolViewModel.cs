// Copyright (C) Scott Kupec. All rights reserved.

using Meringue.AvaDock.ViewModels;

namespace HelloLayout.ViewModels
{
    /// <summary>A customized <see cref="DockItemViewModel"/> for demonstration.</summary>
    public class CustomToolViewModel : DockItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomToolViewModel"/> class.
        /// </summary>
        public CustomToolViewModel()
        {
            // Just to demonstrate the correct class is being constructed.
            System.Diagnostics.Debug.WriteLine($"Constructing {typeof(CustomToolViewModel)}.");
        }
    }
}
