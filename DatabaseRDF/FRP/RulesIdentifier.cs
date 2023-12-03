namespace DatabaseRDF.FRP;

public static class RulesIdentifier
{
    
    private static readonly Dictionary<string, (Func<int, string>, string)> RulesMapper = new()
    {
        {"101", (IsCountryBig, "population")},
        {"isCountryBig", (IsCountryBig, "population")}
    };
    
    public static (Func<int, string>, string) GetRuleMapper(string ruleId)
    {
        return RulesMapper[ruleId];
    }
    
    private static string IsCountryBig(int population)
    {
        return population > 1000000 ? "Country is big" : "Country is small";
    }
}