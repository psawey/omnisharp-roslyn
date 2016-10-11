using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using OmniSharp.Mef;
using OmniSharp.Models;
using OmniSharp.Services;

namespace OmniSharp.Roslyn.CSharp.Services.TestCommands
{
	[Export(typeof(ITestCommandProvider))]
	public class NunitTestCommandProvider : ITestCommandProvider
	{
		private ILogger _logger;

		[ImportingConstructor]
		public NunitTestCommandProvider(ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<NunitTestCommandProvider>();
		}

		private const string All = "nunit3-console.exe --noh {{AssemblyPath}}";
		private const string Fixture = "nunit3-console.exe --noh {{AssemblyPath}} --test={{TypeName}}";
		private const string Single = "nunit3-console.exe --noh {{AssemblyPath}} --test={{TypeName}}.{{MethodName}}";

		public string GetTestCommand(TestContext testContext)
		{
			string testCommand = All;

			switch (testContext.TestCommandType)
			{
				case TestCommandType.All:
					testCommand = All;
					break;
				case TestCommandType.Fixture:
					testCommand = Fixture;
					break;
				case TestCommandType.Single:
					testCommand = Single;
					break;
			}

			var typeName = testContext.Symbol.ContainingType.Name;
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

	[OmniSharpHandler(OmnisharpEndpoints.TestCommand, LanguageNames.CSharp)]
    public class TestCommandService : RequestHandler<TestCommandRequest, GetTestCommandResponse>
    {
        private OmnisharpWorkspace _workspace;
        private readonly List<ITestCommandProvider> _testCommandProviders;

        [ImportingConstructor]
        public TestCommandService(OmnisharpWorkspace workspace, [ImportMany] IEnumerable<ITestCommandProvider> testCommandProviders)
        {
            _workspace = workspace;
	        _testCommandProviders = new List<ITestCommandProvider>(testCommandProviders);
        }

        public async Task<GetTestCommandResponse> Handle(TestCommandRequest request)
        {
            var quickFixes = new List<QuickFix>();

            var document = _workspace.GetDocument(request.FileName);
            var response = new GetTestCommandResponse();
            if (document != null)
            {
                var semanticModel = await document.GetSemanticModelAsync();
                var syntaxTree = semanticModel.SyntaxTree;
                var sourceText = await document.GetTextAsync();
                var position = sourceText.Lines.GetPosition(new LinePosition(request.Line, request.Column));
                var node = syntaxTree.GetRoot().FindToken(position).Parent;

                SyntaxNode method = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                SyntaxNode type = GetTypeDeclaration(node);

                if (type == null)
                {
                    return response;
                }

                var symbol = semanticModel.GetDeclaredSymbol(method ?? type);
                var context = new TestContext(document.Project.FilePath, request.Type, symbol);

                response.TestCommand = _testCommandProviders
                    .Select(t => t.GetTestCommand(context))
                    .FirstOrDefault(c => c != null);

                var directory = Path.GetDirectoryName(document.Project.FilePath);
                response.Directory = directory;
            }

            return response;
        }

        private static SyntaxNode GetTypeDeclaration(SyntaxNode node)
        {
            var type = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            if (type == null)
            {
                type = node.SyntaxTree.GetRoot()
                        .DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault();
            }

            return type;
        }
    }
}
