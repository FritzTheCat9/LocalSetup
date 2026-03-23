namespace LocalSetup.CLI;

public static class SolutionMapper
{
    public static string Normalize(string name)
    {
        return name
            .Replace(".", "")
            .Replace("-", "")
            .ToLower();
    }

    public static List<(string Project, AppResourceMapping? Mapping)> JoinWithSolution(
        List<string> projects,
        List<AppResourceMapping> mappings)
    {
        return projects.Select(p =>
        {
            var match = mappings.FirstOrDefault(m =>
                Normalize(m.AppName).Contains(Normalize(p)) ||
                Normalize(p).Contains(Normalize(m.AppName)));

            return (Project: p, Mapping: match);
        }).ToList();
    }
}