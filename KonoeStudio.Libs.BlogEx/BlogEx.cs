using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.BlogEx.Pipelines;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.IO;
using Wyam.Core.Modules.Metadata;
using Wyam.Feeds;
using Wyam.Web.Pipelines;

namespace Wyam.BlogEx
{
    /// <summary>
    /// A recipe for creating blogging websites.
    /// </summary>
    /// <metadata cref="BlogExKeys.Title" usage="Setting">The title of the blog.</metadata>
    /// <metadata cref="BlogExKeys.Title" usage="Input">The title of the post or page.</metadata>
    /// <metadata cref="BlogExKeys.Image" usage="Setting">The relative path to an image to display on the home page.</metadata>
    /// <metadata cref="BlogExKeys.Image" usage="Input">The relative path to an image for the current post or page (often shown in the header of the page).</metadata>
    /// <metadata cref="BlogExKeys.ProcessIncludes" usage="Setting" />
    /// <metadata cref="BlogExKeys.ProcessIncludes" usage="Input" />
    /// <metadata cref="BlogExKeys.Description" usage="Setting" />
    /// <metadata cref="BlogExKeys.Intro" usage="Setting" />
    /// <metadata cref="BlogExKeys.PostsPath" usage="Setting" />
    /// <metadata cref="BlogExKeys.CaseInsensitiveTags" usage="Setting" />
    /// <metadata cref="BlogExKeys.MarkdownConfiguration" usage="Setting" />
    /// <metadata cref="BlogExKeys.MarkdownExtensionTypes" usage="Setting" />
    /// <metadata cref="BlogExKeys.IncludeDateInPostPath" usage="Setting" />
    /// <metadata cref="BlogExKeys.MetaRefreshRedirects" usage="Setting" />
    /// <metadata cref="BlogExKeys.NetlifyRedirects" usage="Setting" />
    /// <metadata cref="BlogExKeys.RssPath" usage="Setting" />
    /// <metadata cref="BlogExKeys.AtomPath" usage="Setting" />
    /// <metadata cref="BlogExKeys.RdfPath" usage="Setting" />
    /// <metadata cref="BlogExKeys.ValidateAbsoluteLinks" usage="Setting" />
    /// <metadata cref="BlogExKeys.ValidateRelativeLinks" usage="Setting" />
    /// <metadata cref="BlogExKeys.ValidateLinksAsError" usage="Setting" />
    /// <metadata cref="BlogExKeys.TagPageSize" usage="Setting" />
    /// <metadata cref="BlogExKeys.IndexPageSize" usage="Setting" />
    /// <metadata cref="BlogExKeys.IndexPaging" usage="Setting" />
    /// <metadata cref="BlogExKeys.IndexFullPosts" usage="Setting" />
    /// <metadata cref="BlogExKeys.ArchivePageSize" usage="Setting" />
    /// <metadata cref="BlogExKeys.ArchiveExcerpts" usage="Setting" />
    /// <metadata cref="BlogExKeys.GenerateArchive" usage="Setting" />
    /// <metadata cref="BlogExKeys.IgnoreFolders" usage="Setting" />
    /// <metadata cref="BlogExKeys.MarkdownPrependLinkRoot" usage="Setting" />
    /// <metadata cref="BlogExKeys.Published" usage="Input" />
    /// <metadata cref="BlogExKeys.Tags" usage="Input" />
    /// <metadata cref="BlogExKeys.Lead" usage="Input" />
    /// <metadata cref="BlogExKeys.Excerpt" usage="Output" />
    /// <metadata cref="BlogExKeys.ShowInNavbar" usage="Input" />
    /// <metadata cref="BlogExKeys.Posts" usage="Output" />
    /// <metadata cref="BlogExKeys.Tag" usage="Output" />
    public class BlogEx : Recipe
    {
        /// <inheritdoc cref="Web.Pipelines.Pages" />
        [SourceInfo]
        public static Pages Pages { get; } = new Pages(
            nameof(Pages),
            new PagesSettings
            {
                IgnorePaths = ctx =>
                    new[] { ctx.DirectoryPath(BlogExKeys.PostsPath)?.FullPath }
                    .Concat(ctx.List(BlogExKeys.IgnoreFolders, Array.Empty<string>()))
                    .Where(x => x != null),
                MarkdownConfiguration = ctx => ctx.String(BlogExKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BlogExKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BlogExKeys.ProcessIncludes),
                PrependLinkRoot = ctx => ctx.Bool(BlogExKeys.MarkdownPrependLinkRoot)
            });

        /// <inheritdoc cref="Web.Pipelines.BlogPosts" />
        [SourceInfo]
        public static BlogPosts BlogPosts { get; } = new BlogPosts(
            nameof(BlogPosts),
            new BlogPostsSettings
            {
                PublishedKey = BlogExKeys.Published,
                MarkdownConfiguration = ctx => ctx.String(BlogExKeys.MarkdownConfiguration),
                MarkdownExtensionTypes = ctx => ctx.List<Type>(BlogExKeys.MarkdownExtensionTypes),
                ProcessIncludes = (doc, ctx) => doc.Bool(BlogExKeys.ProcessIncludes),
                IncludeDateInPostPath = ctx => ctx.Bool(BlogExKeys.IncludeDateInPostPath),
                PostsPath = ctx => ctx.DirectoryPath(BlogExKeys.PostsPath, ".").FullPath,
                PrependLinkRoot = ctx => ctx.Bool(BlogExKeys.MarkdownPrependLinkRoot)
            });

        /// <summary>
        /// Generates the tag pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive Tags { get; } = new Archive(
            nameof(Tags),
            new ArchiveSettings
            {
                Pipelines = new string[] { BlogPosts },
                TemplateFile = ctx => "_Tag.cshtml",
                Layout = "/_Layout.cshtml",
                Group = (doc, ctx) => doc.List<string>(BlogExKeys.Tags),
                CaseInsensitiveGroupComparer = ctx => ctx.Bool(BlogExKeys.CaseInsensitiveTags),
                PageSize = ctx => ctx.Get(BlogExKeys.TagPageSize, int.MaxValue),
                Title = (doc, ctx) => doc.String(Keys.GroupKey),
                RelativePath = (doc, ctx) => $"tags/{doc.String(Keys.GroupKey)}.html",
                GroupDocumentsMetadataKey = BlogExKeys.Posts,
                GroupKeyMetadataKey = BlogExKeys.Tag
            });

        /// <inheritdoc cref="Pipelines.TagIndex" />
        [SourceInfo]
        public static TagIndex TagIndex { get; } = new TagIndex();

        /// <summary>
        /// Generates the tag pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static Archive Categories { get; } = new Archive(
            nameof(Categories),
            new ArchiveSettings
            {
                Pipelines = new string[] { BlogPosts },
                TemplateFile = ctx => "_Category.cshtml",
                Layout = "/_Layout.cshtml",
                Group = (doc, ctx) => doc.List<string>(BlogExKeys.Categories),
                CaseInsensitiveGroupComparer = ctx => ctx.Bool(BlogExKeys.CaseInsensitiveCategories),
                PageSize = ctx => ctx.Get(BlogExKeys.CategoryPageSize, int.MaxValue),
                Title = (doc, ctx) => doc.String(Keys.GroupKey),
                RelativePath = (doc, ctx) => $"categories/{doc.String(Keys.GroupKey)}.html",
                GroupDocumentsMetadataKey = BlogExKeys.Posts,
                GroupKeyMetadataKey = BlogExKeys.Category
            });

        /// <inheritdoc cref="Pipelines.CategoryIndex" />
        [SourceInfo]
        public static CategoryIndex CategoryIndex { get; } = new CategoryIndex();

        /// <summary>
        /// Generates the index pages for blog posts.
        /// </summary>
        [SourceInfo]
        public static ConditionalPipeline BlogArchive { get; } = new ConditionalPipeline(
            ctx => ctx.Bool(BlogExKeys.GenerateArchive),
            new Archive(
                nameof(BlogArchive),
                new ArchiveSettings
                {
                    Pipelines = new string[] { BlogPosts },
                    TemplateFile = ctx => "_Archive.cshtml",
                    Layout = "/_Layout.cshtml",
                    PageSize = ctx => ctx.Get(BlogExKeys.ArchivePageSize, int.MaxValue),
                    Title = (doc, ctx) => "Archive",
                    RelativePath = (doc, ctx) => $"{ctx.DirectoryPath(BlogExKeys.PostsPath, ".").FullPath}"
                }));

        /// <summary>
        /// Generates the index page(s).
        /// </summary>
        [SourceInfo]
        public static Archive Index { get; } = new Archive(
            nameof(Index),
            new ArchiveSettings
            {
                Pipelines = new string[] { BlogPosts },
                TemplateFile = ctx => ctx.FilePath(BlogExKeys.IndexTemplate, "_Index.cshtml"),
                Layout = "/_Layout.cshtml",
                PageSize = ctx => ctx.Get(BlogExKeys.IndexPageSize, int.MaxValue),
                WriteIfEmpty = true,
                TakePages = ctx => ctx.Bool(BlogExKeys.IndexPaging) ? int.MaxValue : 1,
                RelativePath = (doc, ctx) => $"{ctx.DirectoryPath(BlogExKeys.IndexPath, ".").FullPath}"
            });

        /// <inheritdoc cref="Web.Pipelines.Feeds" />
        [SourceInfo]
        public static Web.Pipelines.Feeds Feed { get; } = new Web.Pipelines.Feeds(
            nameof(Feed),
            new FeedsSettings
            {
                Pipelines = new string[] { BlogPosts },
                RssPath = ctx => ctx.FilePath(BlogExKeys.RssPath),
                AtomPath = ctx => ctx.FilePath(BlogExKeys.AtomPath),
                RdfPath = ctx => ctx.FilePath(BlogExKeys.RdfPath)
            });

        /// <inheritdoc cref="Web.Pipelines.RenderBlogPosts" />
        [SourceInfo]
        public static RenderBlogPosts RenderBlogPosts { get; } = new RenderBlogPosts(
            nameof(RenderBlogPosts),
            new RenderBlogPostsSettings
            {
                Pipelines = new string[] { BlogPosts },
                PublishedKey = BlogExKeys.Published,
                Layout = (doc, ctx) => "/_PostLayout.cshtml"
            });

        /// <inheritdoc cref="Web.Pipelines.RenderPages" />
        [SourceInfo]
        public static RenderPages RenderPages { get; } = new RenderPages(
            nameof(RenderPages),
            new RenderPagesSettings
            {
                Pipelines = new string[] { Pages },
                Layout = (doc, ctx) => "/_Layout.cshtml"
            });

        /// <inheritdoc cref="Web.Pipelines.Redirects" />
        [SourceInfo]
        public static Redirects Redirects { get; } = new Redirects(
            nameof(Redirects),
            new RedirectsSettings
            {
                Pipelines = new string[] { RenderPages, RenderBlogPosts },
                MetaRefreshRedirects = ctx => ctx.Bool(BlogExKeys.MetaRefreshRedirects),
                NetlifyRedirects = ctx => ctx.Bool(BlogExKeys.NetlifyRedirects)
            });

        /// <inheritdoc cref="Web.Pipelines.Less" />
        [SourceInfo]
        public static Web.Pipelines.Less Less { get; } = new Web.Pipelines.Less(nameof(Less));

        /// <inheritdoc cref="Web.Pipelines.Sass" />
        [SourceInfo]
        public static Web.Pipelines.Sass Sass { get; } = new Web.Pipelines.Sass(nameof(Sass));

        /// <inheritdoc cref="Web.Pipelines.Resources" />
        [SourceInfo]
        public static Resources Resources { get; } = new Resources(nameof(Resources));

        /// <inheritdoc cref="Web.Pipelines.ValidateLinks" />
        [SourceInfo]
        public static ValidateLinks ValidateLinks { get; } = new ValidateLinks(
            nameof(ValidateLinks),
            new ValidateLinksSettings
            {
                Pipelines = new string[] { RenderPages, RenderBlogPosts, Resources },
                ValidateAbsoluteLinks = ctx => ctx.Bool(BlogExKeys.ValidateAbsoluteLinks),
                ValidateRelativeLinks = ctx => ctx.Bool(BlogExKeys.ValidateRelativeLinks),
                ValidateLinksAsError = ctx => ctx.Bool(BlogExKeys.ValidateLinksAsError)
            });

        /// <inheritdoc cref="Web.Pipelines.Sitemap" />
        [SourceInfo]
        public static Web.Pipelines.Sitemap Sitemap { get; } = new Web.Pipelines.Sitemap(nameof(Sitemap));

        /// <inheritdoc/>
        public override void Apply(IEngine engine)
        {
            // Global metadata defaults
            engine.Settings[BlogExKeys.Title] = "My Blog";
            engine.Settings[BlogExKeys.Description] = "Welcome!";
            engine.Settings[BlogExKeys.MarkdownConfiguration] = "advanced+bootstrap";
            engine.Settings[BlogExKeys.IncludeDateInPostPath] = false;
            engine.Settings[BlogExKeys.PostsPath] = new DirectoryPath("posts");
            engine.Settings[BlogExKeys.MetaRefreshRedirects] = true;
            engine.Settings[BlogExKeys.GenerateArchive] = true;
            engine.Settings[BlogExKeys.IndexPageSize] = 3;
            engine.Settings[BlogExKeys.RssPath] = GenerateFeeds.DefaultRssPath;
            engine.Settings[BlogExKeys.AtomPath] = GenerateFeeds.DefaultAtomPath;
            engine.Settings[BlogExKeys.RdfPath] = GenerateFeeds.DefaultRdfPath;
            engine.Settings[BlogExKeys.IndexPath] = "index.html";
            engine.Settings[BlogExKeys.IndexTemplate] = "_Index.cshtml";

            base.Apply(engine);
        }

        /// <inheritdoc />
        public override void Scaffold(IFile configFile, IDirectory inputDirectory)
        {
            // Config file
            configFile?.WriteAllText(@"#recipe Blog

// Customize your settings and add new ones here
Settings[Keys.Host] = ""host.com"";
Settings[BlogKeys.Title] = ""My Blog"";
Settings[BlogKeys.Description] = ""Welcome!"";

// Add any pipeline customizations here");

            // Add info page
            inputDirectory.GetFile("about.md").WriteAllText(
@"Title: About Me
---
I'm awesome!");

            // Add post page
            inputDirectory.GetFile("posts/first-post.md").WriteAllText(
@"Title: First Post
Published: 1/1/2016
Tags: Introduction
---
This is my first post!");
        }
    }
}
