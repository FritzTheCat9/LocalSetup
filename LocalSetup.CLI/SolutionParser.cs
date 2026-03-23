using Microsoft.Build.Construction;

namespace LocalSetup.CLI;

public static class SolutionParser
{
    public static List<string> GetProjectNames(string slnPath)
    {
        var sln = SolutionFile.Parse(slnPath);

        return sln.ProjectsInOrder
            .Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
            .Select(p => Path.GetFileNameWithoutExtension(p.AbsolutePath))
            .ToList();
    }
}
