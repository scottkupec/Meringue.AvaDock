// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.IO;
using Meringue.AvaDock.Layout;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// Defines the interface a class must implement to be used as a (de)serialization provider with the docking system.
    /// </summary>
    public interface IDockSerializer
    {
        /// <summary>
        /// Loads <see cref="DockControlData"/> from the specified <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">The name of the file to use for loading the layout.</param>
        /// <returns>The loaded <see cref="DockControlData"/>.</returns>
        DockControlData Load(String filename);

        /// <summary>
        /// Loads <see cref="DockControlData"/> from the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="input">The <see cref="Stream"/> to use for loading the layout.</param>
        /// <returns>The loaded <see cref="DockControlData"/>.</returns>
        DockControlData Load(Stream input);

        /// <summary>
        /// Saves <see cref="DockControlData"/> to the specified <paramref name="filename"/>.
        /// </summary>
        /// <param name="layout">The <see cref="DockControlData"/> to save.</param>
        /// <param name="filename">The name of the file to be used for saving the <see cref="DockControlData"/>.</param>
        void Save(DockControlData layout, String filename);

        /// <summary>
        /// Saves <see cref="DockControlData"/> to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="layout">The <see cref="DockControlData"/> to save.</param>
        /// <param name="output">The <see cref="Stream"/> to be used for saving the <see cref="DockControlData"/>.</param>
        void Save(DockControlData layout, Stream output);
    }
}
