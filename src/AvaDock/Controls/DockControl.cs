// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia;
using Avalonia.Controls.Primitives;
using Meringue.AvaDock.Managers;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Root control that presents a <see cref="DockWorkspace"/> as its visual and contains a <see cref="DockControlManager"/>
    /// to manage it along with any floating window <see cref="DockWorkspace"/> instances it creates.
    /// </summary>
    public class DockControl : TemplatedControl
    {
        /// <summary>Defines the <see cref="Manager"/> property for binding.</summary>
        public static readonly StyledProperty<DockControlManager?> ManagerProperty =
            AvaloniaProperty.Register<DockControl, DockControlManager?>(nameof(Manager));

        /// <summary>
        /// Defines the <see cref="Workspace"/> direct property, which exposes the primary <see cref="DockWorkspaceManager"/>
        /// managed by the current <see cref="Manager"/>. This property is read-only and updates automatically when
        /// <see cref="Manager"/> changes, enabling bindings to track the active workspace presented in the UI.
        /// </summary>
        public static readonly DirectProperty<DockControl, DockWorkspaceManager?> WorkspaceProperty =
            AvaloniaProperty.RegisterDirect<DockControl, DockWorkspaceManager?>(
                nameof(Workspace),
                dockHost => dockHost.Workspace);

        /// <summary>
        /// Initializes static members of the <see cref="DockControl"/> class.
        /// </summary>
        static DockControl()
        {
            _ = ManagerProperty.Changed.AddClassHandler<DockControl>((sender, eventArgs) =>
            {
                sender.RaisePropertyChanged(
                    WorkspaceProperty,
                    oldValue: eventArgs.OldValue is DockControlManager oldHost ? oldHost.PrimaryWorkspace : null,
                    newValue: eventArgs.NewValue is DockControlManager newHost ? newHost.PrimaryWorkspace : null);
            });
        }

        /// <summary>
        /// Gets or sets the <see cref="DockControlManager"/> used to manage the associated <see cref="DockWorkspace"/>s.
        /// </summary>
        public DockControlManager? Manager
        {
            get => this.GetValue(ManagerProperty);
            set => this.SetValue(ManagerProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DockWorkspaceManager"/> used to manage the primary <see cref="DockWorkspace"/>s.
        /// </summary>
        /// <remarks>
        /// This is the visual presented in the UI.
        /// </remarks>
        public DockWorkspaceManager? Workspace => this.Manager?.PrimaryWorkspace;
    }
}
