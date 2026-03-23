namespace LocalSetup.CLI;

public static class EnvironmentParser
{
    public static (string env, string app) Parse(string rg)
    {
        var parts = rg.Split('-', 2);

        if (parts.Length < 2)
            return ("unknown", rg);

        return (parts[0].ToLower(), parts[1]);
    }
}
