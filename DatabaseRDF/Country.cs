using CsvHelper.Configuration.Attributes;

namespace DatabaseRDF;

public class Country
{
    [Name("Country")] public string Name { get; set; }

    [Name("Capital City")] public string Capital { get; set; }

    [Name("Latitude")] public decimal Latitude { get; set; }

    [Name("Longitude")] public decimal Longitude { get; set; }

    [Name("Population")] public int Population { get; set; }

    [Name("Capital Type")] public string CapitalType { get; set; }
}