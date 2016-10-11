using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OmniSharp.Options
{
    public class OmniSharpOptions
    {
        private IDictionary<string, IConfiguration> _items = new Dictionary<string, IConfiguration>();

        public OmniSharpOptions() : this(new FormattingOptions(), new BuildOptions()) { }

        public OmniSharpOptions(FormattingOptions options, BuildOptions buildOptions)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (buildOptions == null)
            {
                throw new ArgumentNullException(nameof(buildOptions));
            }

            FormattingOptions = options;
            BuildOptions = buildOptions;
        }

        public FormattingOptions FormattingOptions { get; }

        public BuildOptions BuildOptions { get; }
    }
}
