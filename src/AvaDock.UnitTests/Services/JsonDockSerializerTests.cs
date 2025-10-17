// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Meringue.AvaDock.Layout;
using Meringue.AvaDock.UnitTests;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Services.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class JsonDockSerializerTests
    {
        [Fact]
        public void Load_ThrowsOnInvalidData()
        {
            JsonDockSerializer serializer = new();
            using MemoryStream stream = new(Encoding.UTF8.GetBytes("Not valid JSON"));

            Should
                .Throw<JsonException>(() => serializer.Load(stream))
                .Message
                .ShouldNotBeNull("Loading invalid json should not succeed.");
        }

        [Fact]
        public void Load_ThrowsOnNullStream()
        {
            JsonDockSerializer serializer = new();

            Should.
                Throw<ArgumentNullException>(() => serializer.Load((Stream)null!))
                .ParamName
                .ShouldBe("input", "Loading null json should not succeed.");
        }

        [Fact]
        public void Save_CanBeLoaded()
        {
            DockItemData itemData = DockLayout.Item("item-id", "Item Title");
            DockTabNodeData tabNodeData = DockLayout.Tab("tab1", itemData);
            DockSplitNodeData splitData = DockLayout.Horizontal(tabNodeData);
            DockWorkspaceData workspaceData = DockLayout.Workspace(splitData);
            DockControlData controlData = DockLayout.Layout(primary: workspaceData);

            JsonDockSerializer serializer = new();

            using MemoryStream stream = new();
            serializer.Save(controlData, stream);
            stream.Seek(0, SeekOrigin.Begin);
            DockControlData result = serializer.Load(stream);

            result
                .ShouldNotBeNull("Loading valid json should succeed.");

            result.Major
                .ShouldBe(controlData.Major, $"{nameof(DockControlData.Major)} should be preserved.");

            result.Minor
                .ShouldBe(controlData.Minor, $"{nameof(DockControlData.Minor)} should be preserved.");

            result.Patch
                .ShouldBe(controlData.Patch, $"{nameof(DockControlData.Patch)} should be preserved.");

            result.PrimaryWorkspace
                .ShouldNotBeNull($"{nameof(DockControlData.PrimaryWorkspace)} should be loaded.");

            result.PrimaryWorkspace.DockTree
                .ShouldNotBeNull($"{nameof(DockControlData.PrimaryWorkspace.DockTree)} should be loaded.");

            result.PrimaryWorkspace.DockTree
                .ShouldNotBeNull($"{nameof(DockControlData.PrimaryWorkspace.DockTree)} should be loaded.");

            result.PrimaryWorkspace.DockTree
                .ShouldBeOfType<DockSplitNodeData>($"{nameof(DockControlData.PrimaryWorkspace.DockTree)} should be the correct type.");

            result.PrimaryWorkspace.DockTree.Sizes.Count
                .ShouldBe(1, $"The correct number of {nameof(DockControlData.PrimaryWorkspace.DockTree.Sizes)} should be present.");

            result.PrimaryWorkspace.DockTree.Children.Count
                .ShouldBe(1, $"The correct number of {nameof(DockControlData.PrimaryWorkspace.DockTree.Children)} should be present.");

            result.PrimaryWorkspace.DockTree.Children[0]
                .ShouldBeOfType<DockTabNodeData>($"The deserialized child should be the correct type.");

            DockTabNodeData? loadedTabNode = result.PrimaryWorkspace.DockTree.Children[0] as DockTabNodeData;

            loadedTabNode!.Tabs
                .ShouldNotBeNull($"{nameof(DockTabNodeData.Tabs)} should be populated.");

            loadedTabNode.Id
                .ShouldNotBeNull($"{nameof(DockTabNodeData.Id)} should be populated.");

            loadedTabNode.Id!
                .ShouldBe(tabNodeData.Id, $"{nameof(DockTabNodeData.Id)} should be populated with the correct value.");

            loadedTabNode.SelectedId
                .ShouldNotBeNull($"{nameof(DockTabNodeData.SelectedId)} should be populated.");

            loadedTabNode.SelectedId!
                .ShouldBe(tabNodeData.SelectedId, $"{nameof(DockTabNodeData.SelectedId)} should be populated with the correct value.");

            loadedTabNode!.Tabs.Count
                .ShouldBe(1, $"{nameof(DockTabNodeData.Tabs)} should be populated with the correct tabs.");

            loadedTabNode!.Tabs[0].Id
                .ShouldBe(itemData.Id, $"{nameof(DockItemData.Id)} should be populated with the oringal value.");

            loadedTabNode!.Tabs[0].Title
                .ShouldBe(itemData.Title, $"{nameof(DockItemData.Title)} should be populated with the oringal value.");
        }

        [Fact]
        public void Save_ThrowsOnNullLayout()
        {
            JsonDockSerializer serializer = new();

            Should
                .Throw<ArgumentNullException>(() => serializer.Save(null!, new MemoryStream()))
                .ParamName
                .ShouldBe("layout", "Saving null json should not succeed.");
        }

        [Fact]
        public void Save_ThrowsOnNullOutputStream()
        {
            JsonDockSerializer serializer = new();
            DockControlData layout = new() { PrimaryWorkspace = new DockWorkspaceData() };

            Should
                .Throw<ArgumentNullException>(() => serializer.Save(layout, (Stream)null!))
                .ParamName
                .ShouldBe("output", "Saving json to a null stream should not succeed.");
        }
    }
}
