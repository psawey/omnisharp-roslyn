using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.Options;

namespace OmniSharp.Abstractions.Services
{
    [Export(typeof(ITestCommandProvider))]
    public class TestCommandProvider : ITestCommandProvider
    {
        private readonly ILogger _logger;
	    private readonly TestCommands _testCommands;

        [ImportingConstructor]
        public TestCommandProvider(ILoggerFactory loggerFactory, TestCommands testCommands)
        {
            _logger = loggerFactory.CreateLogger<TestCommandProvider>();
	        _testCommands = testCommands;
        }

        private const string All = "nunit3-console.exe --noh {{AssemblyPath}}";
        private const string Fixture = "nunit3-console.exe --noh {{AssemblyPath}} --test={{TypeName}}";
        private const string Single = "nunit3-console.exe --noh {{AssemblyPath}} --test={{TypeName}}.{{MethodName}}";

        public string GetTestCommand(TestContext testContext)
        {
            string testCommand = _testCommands.All;

            switch (testContext.TestCommandType)
            {
                case TestCommandType.All:
                    testCommand = _testCommands.All ?? All;
                    break;
                case TestCommandType.Fixture:
                    testCommand = _testCommands.Fixture ?? Fixture;
                    break;
                case TestCommandType.Single:
                    testCommand = _testCommands.Single ?? Single;
                    break;
            }

            var typeName = testContext.Symbol.ContainingSymbol.ToString();
            var directory = new FileInfo(testContext.ProjectFile).Directory.FullName;
            var assembly = testContext.Symbol.ContainingAssembly.Name;
            var methodName = testContext.Symbol.Name;

            var assemblyName = Path.Combine(directory, "bin", "Debug", assembly + ".dll");

            testCommand = testCommand.Replace("{{AssemblyPath}}", assemblyName)
                .Replace("{{TypeName}}", typeName)
                .Replace("{{MethodName}}", methodName);

            _logger.LogInformation("Test Command: '{0}'", testCommand);

            return testCommand;
        }
    }
}