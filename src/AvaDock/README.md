# Meringue.AvaDock

Meringue.AvaDock is a reusable C# and Avalonia 11 control that implements a modern window docking
system. It utilizes an event-driven MVVM architecture and includes a comprehensive serialization
layer along with a built-in JSON serializer.

## Features

- Docking system for Avalonia applications
- Event-driven MVVM architecture using CommunityToolkit.MVVM
- Layout serialization and deserialization
- Modern UI with theming support
- Target frameworks: `netstandard2.0` and `net9.0`

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
1. Use `DockControl` as the root of your docking interface.
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
        public DockControlManager DockControl { get; }
    ```
1. Populate the `DockControl` with your items
    ```csharp
        this.DockControl.AddItem(
            new DockItemViewModel()
            {
                Title = "First item",
                Context = /* your control or data context */,
            });
    ```

See the `Samples` projects for usage examples:
- `HelloDock` - A very basic `DockControl` setup with 4 items.
- `HelloLayout`

## Development Status

Work in progress. The API is evolving. Current focus areas:
- Improve API surface
- Improve layout reflow when moving docked windows
- Final code review before 1.0.0 release

## License

See LICENSE.TXT in the repository root.
