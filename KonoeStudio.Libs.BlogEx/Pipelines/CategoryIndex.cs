using System;
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

namespace Wyam.BlogEx.Pipelines
{
    /// <summary>
    /// Generates the tag index.
    /// </summary>
    public class CategoryIndex : Pipeline
    {
        internal CategoryIndex()
            : base(GetModules())
        {
        }

        private static IModuleList GetModules() => new ModuleList
        {
            new If(
                ctx => ctx.Documents[BlogEx.Categories].Any(),
                new ReadFiles("_Categories.cshtml"),
                new FrontMatter(
                    new Yaml.Yaml()),
                new Shortcodes(true),
                new Razor.Razor()
                    .IgnorePrefix(null)
                    .WithLayout("/_Layout.cshtml"),
                new Shortcodes(false),
                new WriteFiles((doc, ctx) => "categories/index.html"))
            .WithoutUnmatchedDocuments()
        };
    }
}
