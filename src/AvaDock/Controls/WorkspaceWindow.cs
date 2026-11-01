// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Controls;
using Avalonia.Data;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// A window used to host secondary (floated) workspaces.
    /// </summary>
    /// <remarks>
    /// A window subclass is used because the <see cref="WindowBase.Owner"/> property's setter is
    /// not publicly settable in Avalonia 12 so owned windows can only be created through a sub-class.
    /// Application styles are inherited automatically by every top-level window, so no style re-anchoring
    /// is needed; only the requested theme variant is synced, bound one-way to the owner so runtime
    /// variant switches on the owner are followed.
    /// </remarks>
    public class WorkspaceWindow : Window
    {
        /// <summary>Initializes a new instance of the <see cref="WorkspaceWindow"/> class.</summary>
        public WorkspaceWindow()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceWindow"/> class.
        /// </summary>
        /// <param name="owner">The owning window.</param>
        public WorkspaceWindow(Window owner)
            : this()
        {
            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.BindThemeToOwner();
        }

        /// <summary>Binds an instance with an owning window to the owning windows theme.</summary>
        private void BindThemeToOwner()
        {
            if (this.Owner is not null)
            {
                _ = this.Bind(
                    RequestedThemeVariantProperty,
                    new Binding
                    {
                        Source = this.Owner,
                        Path = nameof(Window.RequestedThemeVariant),
                        Mode = BindingMode.OneWay,
                    });
            }
        }
    }
}
