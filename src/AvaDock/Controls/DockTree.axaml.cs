// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Controls;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Represents either a <see cref="DockTabPanel"/> or a <see cref="DockSplitPanel"/> in the
    /// docking controls tree.
    /// </summary>
    public partial class DockTree : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockTree"/> class.
        /// </summary>
        public DockTree()
            => this.InitializeComponent();
    }
}
