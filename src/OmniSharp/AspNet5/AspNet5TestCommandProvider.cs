using Microsoft.CodeAnalysis;
using System.IO;

namespace OmniSharp.AspNet5
{
    public class AspNet5TestCommandProvider : ITestCommandProvider
    {
        private readonly AspNet5Context _context;
        
        public AspNet5TestCommandProvider(AspNet5Context context)
        {
            _context = context;
        }
        
        public string GetTestCommand(TestContext testContext)
        {
            if (!_context.ProjectContextMapping.ContainsKey(testContext.ProjectFile))
            {
                return null;
            }

            var projectCounter = _context.ProjectContextMapping[testContext.ProjectFile];
            var project = _context.Projects[projectCounter];

            if (!project.Commands.ContainsKey("test"))
            {
                return null;
            }

            // Find the test command, if any and use that
            var symbol = testContext.Symbol;
            string testsToRun = "";
            
            if (symbol is IMethodSymbol)
            {
                testsToRun = symbol.ContainingType.Name + "." + symbol.Name;
            }
            else if (symbol is INamedTypeSymbol)
            {
                testsToRun = symbol.Name;
            }

            if (!symbol.ContainingNamespace.IsGlobalNamespace)
            {
                testsToRun = symbol.ContainingNamespace + "." + testsToRun;
            }

            string kCommand = Path.Combine(_context.RuntimePath, "bin", "k");
            
            switch (testContext.TestCommandType)
            {
                case TestCommandType.All:
                    kCommand = kCommand + " test";
                    break;
                case TestCommandType.Single:
                case TestCommandType.Fixture:
                    kCommand = kCommand + " test --test " + testsToRun;
                    break;
            }
            return kCommand;
        }
    }
}