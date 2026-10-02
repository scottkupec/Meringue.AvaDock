# Meringue.AvaDock

Meringue.AvaDock is a reusable C# and Avalonia 12 control that implements a simple window docking
system. It utilizes an event-driven MVVM architecture and includes a comprehensive serialization
layer along with a built-in JSON serializer.

## Features

- Docking system for Avalonia applications
- Event-driven MVVM architecture using CommunityToolkit.MVVM
- Layout serialization and deserialization
- Modern UI with theming support
- Split panels top, bottom, right, left or stack panels into a switchable tab control
- Panels can be minimized, hidden, or closed.
    - Maximize not yet implemented
- Float panels via title bar drop down
    - Drag to float not yet implemented

## Quick Start

1. Add Add the package to your Avalonia project in `Directory.Packages.props` or other mechanism.
1. Include the necessary theming in App.axaml
    ```xml
    <Application
      ...
      xmlns:dock="clr-namespace:Meringue.AvaDock.Themes;assembly=Meringue.AvaDock">

      <Application.Styles>
        <dock:AvaDockTheme />
      </Application.Styles>
    </Application>
    ```
1. Use a `DockControl` instance as the root of your docking interface.
    ```xml
    <Window xmlns="https://github.com/avaloniaui"
            xmlns:dock="clr-namespace:Meringue.AvaDock.Controls;assembly=Meringue.AvaDock"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
      ...

      <dock:DockControl Manager="{Binding DockManager}" />

      ....
    </Window>
    ```
1. Add a DockManager in the code behind to manage the control:
    ```csharp
        public DockControlManager DockManager { get; }
    ```
1. Populate the `DockControl` with your items
    ```csharp
        this.DockManager.AddItem(
            new DockItemViewModel()
            {
                Title = "First item",
                Context = /* your control or data context */,
            });
    ```

See the `Samples` projects for usage examples:
- `HelloDock` - A basic `DockControl` setup with 4 items.
- `HelloLayout` - Uses serialized JSON to construct a `DockControl`.
- `LayoutMonitor` - A layout sample that monitors changes to the layout.

## Development Status

Work in progress. The API is evolving. Current focus areas:
- Improve API surface
- Improve layout reflow when moving docked windows
- Add drag-to-float instead of just a "Float" drop down.
- Better support when restoring contexts on deserialization.

## License

See LICENSE.TXT in the repository root.
