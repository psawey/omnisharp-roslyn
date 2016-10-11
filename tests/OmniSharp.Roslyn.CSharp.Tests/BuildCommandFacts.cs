using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniSharp.Abstractions.Models.v1;
using OmniSharp.Roslyn.CSharp.Services.Build;
using TestUtility.Fake;
using Xunit;

namespace OmniSharp.Roslyn.CSharp.Tests
{
    public class BuildCommandFacts
    {
        private const string _buildConfig = "~/src/";

        [Fact]
        public async Task Empty_build_config_uses_current_directory()
        {
            var actual = await SendBuildRequest();

            Assert.NotNull(actual);
            Assert.DoesNotContain(_buildConfig, actual);
        }

        [Fact]
        public async Task Included_build_config_uses_config_value()
        {
            var actual = await SendBuildRequest(_buildConfig);

            Assert.NotNull(actual);
            Assert.Contains(_buildConfig, actual);
        }

        private async Task<string> SendBuildRequest()
        {
            return await SendBuildRequest(null);
        }

        private async Task<string> SendBuildRequest(string buildConfig)
        {
            var fakeEnvironment = new FakeEnvironment();
            var fakeLoggerFactory = new FakeLoggerFactory();
            var fakeBuildOptions = new FakeOmniSharpOptions().Value;

            fakeBuildOptions.BuildOptions.MsBuildPath = buildConfig;

            var controller = new BuildCommandService(fakeEnvironment, fakeLoggerFactory, fakeBuildOptions.BuildOptions);

            return await controller.Handle(new BuildCommandRequest());
        }
    }
}
