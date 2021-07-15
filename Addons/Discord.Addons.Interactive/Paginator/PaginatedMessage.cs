using System;
using System.Collections.Generic;

namespace Discord.Addons.Interactive
{
    /// <summary>
    /// Paginated message model
    /// </summary>
    public class PaginatedMessage
    {
        /// <summary>
        /// Gets or sets the pages.
        /// </summary>
        public IEnumerable<Page> Pages { get; set; } = new List<Page>();

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public EmbedAuthorBuilder Author { get; set; } = null;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = null;

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; } = null;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image url.
        /// </summary>
        public string ImageUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the thumbnail url.
        /// </summary>
        public string ThumbnailUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public List<EmbedFieldBuilder> Fields { get; set; } = new List<EmbedFieldBuilder>();

        /// <summary>
        /// Footer text override. IMPORTANT! The template must contains "{0}" for the current page and "{1}" for the total number of pages. For example "Wish {0}/{1}"
        /// </summary>
        public string FooterTextOverride { get; set; } = null;

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        public DateTimeOffset? TimeStamp { get; set; } = null;

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }

    /// <summary>
    /// Page model for <see cref="PaginatedMessage"/>
    /// </summary>
    public class Page
    {
        // All content in here will override the 'Default' Paginated content

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public EmbedAuthorBuilder Author { get; set; } = null;

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = null;

        /// <summary>
        /// Gets or sets the url.
        /// </summary>
        public string Url { get; set; } = null;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// Gets or sets the image url.
        /// </summary>
        public string ImageUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the thumbnail url.
        /// </summary>
        public string ThumbnailUrl { get; set; } = null;

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        public List<EmbedFieldBuilder> Fields { get; set; } = new List<EmbedFieldBuilder>();

        /// <summary>
        /// Gets or sets the footer override.
        /// </summary>
        public string FooterTextOverride { get; set; } = null;

        /// <summary>
        /// Gets or sets the time stamp.
        /// </summary>
        public DateTimeOffset? TimeStamp { get; set; } = null;

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color? Color { get; set; } = null;
    }
}
