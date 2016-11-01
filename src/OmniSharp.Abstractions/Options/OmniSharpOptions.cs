using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OmniSharp.Options
{
    public class OmniSharpOptions
    {
        private IDictionary<string, IConfiguration> _items = new Dictionary<string, IConfiguration>();

        public OmniSharpOptions() : this(new FormattingOptions(), new TestCommands()) { }

        public OmniSharpOptions(FormattingOptions options, TestCommands testCommands)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (testCommands == null)
            {
                throw new ArgumentNullException(nameof(testCommands));
            }

            FormattingOptions = options;
            TestCommands = testCommands;
        }

        public FormattingOptions FormattingOptions { get; }

        public TestCommands TestCommands { get; }
    }
}
