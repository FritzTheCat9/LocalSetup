namespace LocalSetup.CLI;

public class FunctionAppConfig
{
    public string Name { get; set; } = "";

    public IDictionary<string, string> Settings { get; set; }
        = new Dictionary<string, string>();
}
