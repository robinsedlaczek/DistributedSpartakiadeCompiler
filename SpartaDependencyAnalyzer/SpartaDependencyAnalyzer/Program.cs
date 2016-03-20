using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace SpartaDependencyAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            var solutionPath = @"D:\Temp\CompilationTestSolution\TestSolution.sln";
            var workspace = MSBuildWorkspace.Create();

            var solution = workspace.OpenSolutionAsync(solutionPath).Result;
            var dependencyGraph = solution.GetProjectDependencyGraph();
            var topo = dependencyGraph.GetTopologicallySortedProjects();

            var projectRefMapping = new Dictionary<ProjectId, string>();

            var tmpFolder = @"D:\Temp\CompileTestFolder";

            foreach (var projectId in topo)
            {
                var project = solution.Projects.First(p => p.Id == projectId);

                foreach (var projectReference in project.ProjectReferences)
                {
                    project = ReplaceProjectDependency(project, projectReference,
                    projectRefMapping[projectReference.ProjectId]);
                }

                var tmpFile = Path.Combine(tmpFolder, project.AssemblyName);
                tmpFile += project.CompilationOptions.OutputKind == OutputKind.DynamicallyLinkedLibrary
                    ? ".dll"
                    : ".exe";

                var result = CompileProject(project, tmpFile);

                if (!result.Success)
                    throw new Exception("Not successful");

                projectRefMapping.Add(project.Id, tmpFile);
            }

            Console.WriteLine("Done");
            Console.Read();
        }

        private static Project ReplaceProjectDependency(Project project, ProjectReference projectDependency, string assemblyFile)
        {
            project = project.RemoveProjectReference(projectDependency);
            var metaRef = MetadataReference.CreateFromFile(assemblyFile);

            return project.AddMetadataReference(metaRef);
        }

        private static EmitResult CompileProject(Project project, string filename)
        {
            var compilationTask = project.GetCompilationAsync().ConfigureAwait(false);
            
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                return compilationTask.GetAwaiter().GetResult().Emit(stream);
            }
            
        }
    }
}
