using CsvHelper.Configuration.Attributes;

namespace DatabaseRDF;

public class Country
{
    public Country(int population, decimal longitude, decimal latitude, string name, string capital, string capitalType)
    {
        Population = population;
        Longitude = longitude;
        Latitude = latitude;
        Name = name;
        Capital = capital;
        CapitalType = capitalType;
    }

    [Name("Country")] public string Name { get; } 

    [Name("Capital City")] public string Capital { get; } 

    [Name("Latitude")] public decimal Latitude { get; } 

    [Name("Longitude")] public decimal Longitude { get; } 

    [Name("Population")] public int Population { get; } 

    [Name("Capital Type")] public string CapitalType { get; } 
}