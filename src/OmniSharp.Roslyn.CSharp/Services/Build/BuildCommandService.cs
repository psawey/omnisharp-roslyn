using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Options;
using OmniSharp.Services;

namespace OmniSharp.Roslyn.CSharp.Services.Build
{
    [OmniSharpHandler(OmnisharpEndpoints.BuildCommand, LanguageNames.CSharp)]
    public class BuildCommandService : RequestHandler<Request, string>
    {
        private readonly IOmnisharpEnvironment _environment;
        private readonly ILogger _logger;
        private readonly BuildOptions _buildOptions;

        [ImportingConstructor]
        public BuildCommandService(IOmnisharpEnvironment environment, ILoggerFactory loggerFactory, BuildOptions buildOptions)
        {
            _environment = environment;
            _logger = loggerFactory.CreateLogger<BuildCommandService>();
            _buildOptions = buildOptions;
        }

        public Task<string> Handle(Request request)
        {
            var buildCommandResult = BuildExecutable() + " " + BuildArguments();

            _logger.LogDebug("Build Command Result: '{0}'", buildCommandResult);

            return Task.FromResult(buildCommandResult);
        }

        private string BuildExecutable()
        {
            var buildPath = PlatformHelper.IsMono
                ? "xbuild"
                : Path.Combine(_buildOptions.MsBuildPath ?? Directory.GetCurrentDirectory(), "msbuild.exe");

            return buildPath;
        }

        private string BuildArguments()
        {
            return (PlatformHelper.IsMono ? "" : "/m ") + "/nologo /v:q /property:GenerateFullPaths=true \"" + BuildSolutionFileName() + "\"";
        }

        private string BuildSolutionFileName()
        {
            return _environment.SolutionFilePath ?? Directory.GetFiles(_environment.Path, "*.sln").FirstOrDefault();
        }
    }
}
