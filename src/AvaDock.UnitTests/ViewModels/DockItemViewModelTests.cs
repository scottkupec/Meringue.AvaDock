// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Meringue.AvaDock.Events;
using Shouldly;
using Xunit;

namespace Meringue.AvaDock.ViewModels.UnitTests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public sealed class DockItemViewModelTests
    {
        [Fact]
        public void CloseCommand_ShouldNotExecuteWhenDisableCloseIsTrue()
        {
            DockItemViewModel item = new() { DisableClose = true };
            Boolean eventRaised = false;

            item.CloseRequested += (_, _) => eventRaised = true;

            Boolean canExecute = item.CloseCommand.CanExecute(null);
            item.CloseCommand.Execute(null);

            canExecute
                .ShouldBeFalse($"{nameof(item.CloseCommand)} should not be executable when {nameof(item.DisableClose)} is true");

            eventRaised
                .ShouldBeFalse($"{nameof(item.CloseRequested)} event should not be raised");
        }

        [Fact]
        public void CloseCommand_ShouldRaiseCloseRequestedWhenDisableCloseIsFalse()
        {
            DockItemViewModel item = new() { DisableClose = false };
            DockItemCloseRequestedEventArgs? receivedArgs = null;

            item.CloseRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.CloseCommand.CanExecute(null);
            item.CloseCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.CloseCommand)} should be executable when {nameof(item.DisableClose)} is false");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.CloseRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemCloseRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void Constructor_SetsDefaults()
        {
            DockItemViewModel viewModel = new();

            viewModel.Context
                .ShouldBeNull($"The default value for {nameof(DockItemViewModel.Context)} should be correct");

            viewModel.Title
                .ShouldBe("[Untitled]", $"The default value for {nameof(DockItemViewModel.Title)} should be correct");

            Guid.TryParse(viewModel.Id, out _)
                .ShouldBeTrue($"The default value for {nameof(DockItemViewModel.Id)} should be correct");
        }

        [Fact]
        public void Context_AssignmentPreservesValue()
        {
            DockItemViewModel viewModel = new();
            Object content = new { Message = "Hello" };

            viewModel.Context = content;

            viewModel.Context
                .ShouldBe(content, $"Assignment of {nameof(viewModel.Context)} should preserve the value set");
        }

        [Fact]
        public void FloatCommand_ShouldRaiseFloatRequested()
        {
            DockItemViewModel item = new();
            DockItemFloatRequestedEventArgs? receivedArgs = null;

            item.FloatRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.FloatCommand.CanExecute(null);
            item.FloatCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.FloatCommand)} should be executable");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.FloatRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemFloatRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void HideCommand_ShouldNotExecuteWhenDisableHideIsTrue()
        {
            DockItemViewModel item = new() { DisableHide = true };
            Boolean eventRaised = false;

            item.HideRequested += (_, _) => eventRaised = true;

            Boolean canExecute = item.HideCommand.CanExecute(null);
            item.HideCommand.Execute(null);

            canExecute
                .ShouldBeFalse($"{nameof(item.HideCommand)} should not be executable when {nameof(item.DisableHide)} is true");

            eventRaised
                .ShouldBeFalse($"{nameof(item.HideRequested)} event should not be raised");
        }

        [Fact]
        public void HideCommand_ShouldRaiseHideRequestedWhenDisableHideIsFalse()
        {
            DockItemViewModel item = new() { DisableHide = false };
            DockItemHideRequestedEventArgs? receivedArgs = null;

            item.HideRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.HideCommand.CanExecute(null);
            item.HideCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.HideCommand)} should be executable when {nameof(item.DisableHide)} is false");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.HideRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemHideRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void Id_AssignmentPreservesValue()
        {
            String id = "custom-item-id";

            DockItemViewModel viewModel = new()
            {
                Id = id,
            };

            viewModel.Id
                .ShouldBe(id, $"Assignment of {nameof(viewModel.Id)} should preserve the value set");
        }

        [Fact]
        public void MaximizeCommand_ShouldNotExecuteWhenDisableMaximizeIsTrue()
        {
            DockItemViewModel item = new() { DisableMaximize = true };
            Boolean eventRaised = false;

            item.MaximizeRequested += (_, _) => eventRaised = true;

            Boolean canExecute = item.MaximizeCommand.CanExecute(null);
            item.MaximizeCommand.Execute(null);

            canExecute
                .ShouldBeFalse($"{nameof(item.MaximizeCommand)} should not be executable when {nameof(item.DisableMaximize)} is true");

            eventRaised
                .ShouldBeFalse($"{nameof(item.MaximizeRequested)} event should not be raised");
        }

        [Fact]
        public void MaximizeCommand_ShouldRaiseMaximizeRequestedWhenDisableMaximizeIsFalse()
        {
            DockItemViewModel item = new() { DisableMaximize = false };
            DockItemMaximizeRequestedEventArgs? receivedArgs = null;

            item.MaximizeRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.MaximizeCommand.CanExecute(null);
            item.MaximizeCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.MaximizeCommand)} should be executable when {nameof(item.DisableMaximize)} is false");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.MaximizeRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemMaximizeRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void MinimizeCommand_ShouldNotExecuteWhenDisableMinimizeIsTrue()
        {
            DockItemViewModel item = new() { DisableMinimize = true };
            Boolean eventRaised = false;

            item.MinimizeRequested += (_, _) => eventRaised = true;

            Boolean canExecute = item.MinimizeCommand.CanExecute(null);
            item.MinimizeCommand.Execute(null);

            canExecute
                .ShouldBeFalse($"{nameof(item.MinimizeCommand)} should not be executable when {nameof(item.DisableMinimize)} is true");

            eventRaised
                .ShouldBeFalse($"{nameof(item.MinimizeRequested)} event should not be raised");
        }

        [Fact]
        public void MinimizeCommand_ShouldRaiseMinimizeRequestedWhenDisableMinimizeIsTrue()
        {
            DockItemViewModel item = new() { DisableMinimize = false };
            DockItemMinimizeRequestedEventArgs? receivedArgs = null;

            item.MinimizeRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.MinimizeCommand.CanExecute(null);
            item.MinimizeCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.MinimizeCommand)} should be executable when {nameof(item.DisableMinimize)} is false");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.MinimizeRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemMinimizeRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void RestoreCommand_ShouldRaiseRestoreRequested()
        {
            DockItemViewModel item = new();
            DockItemRestoreRequestedEventArgs? receivedArgs = null;

            item.RestoreRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.RestoreCommand.CanExecute(null);
            item.RestoreCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.RestoreCommand)} should be executable");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.RestoreRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemRestoreRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void ShowCommand_ShouldRaiseShowRequested()
        {
            DockItemViewModel item = new();
            DockItemShowRequestedEventArgs? receivedArgs = null;

            item.ShowRequested += (_, args) => receivedArgs = args;

            Boolean canExecute = item.ShowCommand.CanExecute(null);
            item.ShowCommand.Execute(null);

            canExecute
                .ShouldBeTrue($"{nameof(item.ShowCommand)} should be executable");

            receivedArgs
                .ShouldNotBeNull($"{nameof(item.ShowRequested)} event should be raised");

            receivedArgs!.Item
                .ShouldBe(item, $"{nameof(DockItemShowRequestedEventArgs)} should reference the correct {nameof(DockItemViewModel)}");
        }

        [Fact]
        public void Title_AssignmentPreservesValue()
        {
            String title = "Item Title";

            DockItemViewModel viewModel = new()
            {
                Title = title,
            };

            viewModel.Title
                .ShouldBe(title, $"Assignment of {nameof(viewModel.Title)} should preserve the value set");
        }
    }
}
