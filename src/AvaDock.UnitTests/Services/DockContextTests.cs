// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia.Layout;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.ViewModels;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.Services.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockContextTests
    {
        [Fact]
        public void ClearPreferredTabPanelId_RemovesTag()
        {
            DockItemViewModel item = new();
            DockContext.SetPreferredTabPanelId(item, "ToBeRemoved");

            DockContext.ClearPreferredTabPanelId(item);
            String? returnedId = DockContext.GetPreferredTabPanelId(item);

            returnedId
                .ShouldBeNull($"{nameof(DockContext.GetPreferredTabPanelId)} should return null after {DockContext.ClearPreferredTabPanelId} is called.");
        }

        [Fact]
        public void ClearPreferredWorkspaceId_RemovesTag()
        {
            DockItemViewModel item = new();
            DockContext.SetPreferredWorkspaceId(item, "tab-panel");

            DockContext.ClearPreferredWorkspaceId(item);

            String? returnedId = DockContext.GetPreferredWorkspaceId(item);

            returnedId
                .ShouldBeNull($"{nameof(DockContext.GetPreferredWorkspaceId)} should return null after {DockContext.ClearPreferredWorkspaceId} is called.");
        }

        [Fact]
        public void ClearWorkspace_RemovesTag()
        {
            DockItemViewModel item = new();
            DockWorkspaceManager workspace = new(new DockSplitNodeViewModel(Orientation.Vertical));
            DockContext.SetWorkspace(item, workspace);

            DockContext.ClearWorkspace(item);

            DockWorkspaceManager? returnedManager = DockContext.GetWorkspace(item);

            returnedManager
                .ShouldBeNull($"{nameof(DockContext.GetWorkspace)} should return null after {DockContext.ClearWorkspace} is called.");
        }

        [Fact]
        public void GetDockHost_ReturnsNullWhenNotSet()
        {
            DockItemViewModel item = new();
            DockControlManager? result = DockContext.GetDockHost(item);

            result
                .ShouldBeNull($"{nameof(DockContext.GetDockHost)} should return null when no value is set.");
        }

        [Fact]
        public void GetWorkspace_ReturnsNullWhenNotSet()
        {
            DockItemViewModel item = new();
            DockWorkspaceManager? result = DockContext.GetWorkspace(item);

            result
                .ShouldBeNull($"{nameof(DockContext.GetWorkspace)} should return null when no value is set.");
        }

        [Fact]
        public void GetPreferredTabPanelId_ReturnsNullWhenNotSet()
        {
            DockItemViewModel item = new();
            String? result = DockContext.GetPreferredTabPanelId(item);

            result
                .ShouldBeNull($"{nameof(DockContext.GetPreferredTabPanelId)} should return null when no value is set.");
        }

        [Fact]
        public void GetPreferredWorkspaceId_ReturnsNullWhenNotSet()
        {
            DockItemViewModel item = new();
            String? result = DockContext.GetPreferredWorkspaceId(item);

            result
                .ShouldBeNull($"{nameof(DockContext.GetPreferredWorkspaceId)} should return null when no value is set.");
        }

        [Fact]
        public void SetDockHost_CanBeRetrieved()
        {
            DockItemViewModel item = new();
            DockControlManager host = new(new DockWorkspaceManager(new DockSplitNodeViewModel(Orientation.Vertical)));

            DockContext.SetDockHost(item, host);
            DockControlManager? storedHost = DockContext.GetDockHost(item);

            storedHost
                .ShouldNotBeNull($"Storing a {nameof(DockControlManager)} should allow it to be returned.");

            storedHost
                .ShouldBe(host, $"Tag value should match the assigned {nameof(DockControlManager)} instance.");
        }

        [Fact]
        public void SetPreferredTabPanelId_CanBeRetrieved()
        {
            DockItemViewModel item = new();
            String expectedId = "TabPanel123";

            DockContext.SetPreferredTabPanelId(item, expectedId);
            String? storedId = DockContext.GetPreferredTabPanelId(item);

            storedId
                .ShouldNotBeNull($"An ID stored with {nameof(DockContext.GetPreferredTabPanelId)} should be retrievable.");

            storedId
                .ShouldBe(expectedId, $"Tag value should match value used with {nameof(DockContext.SetPreferredTabPanelId)}.");
        }

        [Fact]
        public void SetPreferredWorkspaceId_CanBeRetrieved()
        {
            DockItemViewModel item = new();
            String expectedId = "WorkspaceABC";

            DockContext.SetPreferredWorkspaceId(item, expectedId);
            String? storedId = DockContext.GetPreferredWorkspaceId(item);

            storedId
                .ShouldNotBeNull($"An ID stored with {nameof(DockContext.GetPreferredWorkspaceId)} should be retrievable.");

            storedId
                .ShouldBe(expectedId, $"Tag value should match value used with {nameof(DockContext.SetPreferredWorkspaceId)}.");
        }

        [Fact]
        public void SetWorkspace_CanBeRetrieved()
        {
            DockItemViewModel item = new();
            DockWorkspaceManager manager = new(new DockSplitNodeViewModel(Orientation.Vertical));

            DockContext.SetWorkspace(item, manager);
            DockWorkspaceManager? storedManager = DockContext.GetWorkspace(item);

            storedManager
                .ShouldNotBeNull($"Storing a {nameof(DockWorkspaceManager)} should allow it to be returned.");

            storedManager
                .ShouldBe(manager, $"Tag value should match the assigned {nameof(DockWorkspaceManager)} instance.");
        }
    }
}
