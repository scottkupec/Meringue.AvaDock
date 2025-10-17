// Copyright (C) Scott Kupec. All rights reserved.

using System;
using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Meringue.AvaDock.Themes
{
    /// <summary>
    /// Theme file for the docking system that allows consumers to just use
    ///     &lt;dock:AvaDockTheme /&gt;
    /// in their App.axaml and get all of the necessary themes and data templates.
    /// </summary>
    public class AvaDockTheme : Styles
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvaDockTheme"/> class.
        /// </summary>
        public AvaDockTheme()
        {
            AvaDockTheme.InjectTemplates(new Uri("avares://Meringue.AvaDock/Themes/DataTemplates.axaml"));
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Injects the data templates into the current application.
        /// </summary>
        /// <param name="templateUri">The uri of the templates to inject.</param>
        public static void InjectTemplates(Uri templateUri)
        {
            if (Application.Current is { } app)
            {
                DataTemplates templates = (DataTemplates)AvaloniaXamlLoader.Load(templateUri);
                app.DataTemplates.AddRange(templates);
            }
        }
    }
}
