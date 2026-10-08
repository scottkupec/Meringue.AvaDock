// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Meringue.AvaDock.Layout;

namespace Meringue.AvaDock.Services
{
    /// <summary>
    /// An <see cref="IDockSerializer"/> implementation using System.Text.Json.
    /// </summary>
    // TODO: Task 178 to support passing JsonSerializerOptions to load/save.
    public class JsonDockSerializer : IDockSerializer
    {
        /// <summary>
        /// Defines the <see cref="JsonSerializerOptions"/> used when loading and saving JSON.
        /// </summary>
        private static readonly JsonSerializerOptions Options = new()
        {
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        /// <inheritdoc/>
        public DockControlData Load(String filename)
        {
            // CONSIDER: Custom exception wrapper so we log filename in the exception as well.
            using FileStream stream = File.OpenRead(filename);
            return this.Load(stream);
        }

        /// <inheritdoc/>
        public DockControlData Load(Stream input)
        {
            ArgumentNullException.ThrowIfNull(input);

            DockControlData? layout = JsonSerializer.Deserialize<DockControlData>(input, Options);
            return layout ?? throw new InvalidDataException("Could not deserialize DockLayout.");
        }

        /// <inheritdoc/>
        public void Save(DockControlData layout, String filename)
        {
            ArgumentNullException.ThrowIfNull(layout);

            using FileStream stream = File.Create(filename);
            this.Save(layout, stream);
        }

        /// <inheritdoc/>
        public void Save(DockControlData layout, Stream output)
        {
            ArgumentNullException.ThrowIfNull(layout);
            ArgumentNullException.ThrowIfNull(output);

            JsonSerializer.Serialize(output, layout, Options);
        }
    }
}
