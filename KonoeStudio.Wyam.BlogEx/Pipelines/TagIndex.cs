﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.IO;
using Wyam.Razor;
using Wyam.Yaml;

namespace KonoeStudio.Wyam.BlogEx.Pipelines
{
    /// <summary>
    /// Generates the tag index.
    /// </summary>
    public class TagIndex : Pipeline
    {
        internal TagIndex()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(
                ctx => ctx.Documents[BlogEx.Tags].Any(),
                new ReadFiles("_Tags.cshtml"),
                new FrontMatter(
                    new Yaml()),
                new Shortcodes(true),
                new Razor()
                    .IgnorePrefix(null)
                    .WithLayout("/_Layout.cshtml"),
                new Shortcodes(false),
                new WriteFiles((doc, ctx) => "tags/index.html"))
            .WithoutUnmatchedDocuments()
        };
    }
}
