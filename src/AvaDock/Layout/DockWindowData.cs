// Copyright (C) Scott Kupec. All rights reserved.

using System;
using System.Text.Json.Serialization;
using Avalonia;
using Meringue.AvaDock.Managers;
using Meringue.AvaDock.Services;

namespace Meringue.AvaDock.Layout
{
    /// <summary>
    /// Represents the serializable state of an <see cref="IWindow"/> hosting a <see cref="DockWorkspaceManager"/>.
    /// </summary>
    public sealed class DockWindowData
    {
        /// <summary>
        /// Gets or sets the height of the the <see cref="IWindow"/>.
        /// </summary>
        public Double Height { get; set; }

        /// <summary>
        /// Gets or sets the left edge coordinate of the the <see cref="IWindow"/>.
        /// </summary>
        public Int32 Left { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IWindow.Position"/> of the the <see cref="IWindow"/>.
        /// </summary>
        [JsonIgnore]
        public PixelPoint Position
        {
            get => new(this.Left, this.Top);
            set
            {
                this.Left = value.X;
                this.Top = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the height and width of the the <see cref="IWindow"/>.
        /// </summary>
        [JsonIgnore]
        public Size Size
        {
            get => new(this.Width, this.Height);
            set
            {
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }

        /// <summary>
        /// Gets or sets the top edge coordinate of the the <see cref="IWindow"/>.
        /// </summary>
        public Int32 Top { get; set; }

        /// <summary>
        /// Gets or sets the width of the the <see cref="IWindow"/>.
        /// </summary>
        public Double Width { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DockWorkspaceData"/> for the <see cref="IWindow"/>.
        /// </summary>
        public DockWorkspaceData? Workspace { get; set; }
    }
}
