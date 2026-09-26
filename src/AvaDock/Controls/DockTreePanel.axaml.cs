// Copyright (C) Scott Kupec. All rights reserved.

using Avalonia.Controls;

namespace Meringue.AvaDock.Controls
{
    /// <summary>
    /// Represents either a <see cref="DockTabPanel"/> or a <see cref="DockSplitPanel"/> in the
    /// docking controls tree.
    /// </summary>
    public partial class DockTreePanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockTreePanel"/> class.
        /// </summary>
        public DockTreePanel()
            => this.InitializeComponent();
    }
}
