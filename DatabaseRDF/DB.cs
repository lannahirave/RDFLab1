using System.Globalization;
using System.Reflection;
using CsvHelper;
using DatabaseRDF.Exceptions;
using DatabaseRDF.FRP;
using VDS.RDF;

namespace DatabaseRDF;

public class DatabaseRdf
{
    private static DatabaseRdf? _instance;

    private static readonly object Lock = new();

    private IGraph Graph { get; init; } = null!;

    public static DatabaseRdf GetInstance()
    {
        if (_instance is null)
            lock (Lock)
            {
                _instance ??= new DatabaseRdf
                {
                    Graph = new Graph()
                };
            }

        return _instance;
    }


    public static void LoadDataFromDefaultCsvFile()
    {
        var filePath =
            Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                throw new InvalidOperationException(), @"DataRaw\CountriesData.csv");

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var records = csv.GetRecords<Country>();

        // Отримуємо граф
        var graph = GetInstance().Graph;

        // Створюємо вузли для предикатів

        var namePredicate = graph.CreateUriNode(new Uri("http://data.countries/name"));
        var capitalPredicate = graph.CreateUriNode(new Uri("http://data.countries/capital"));
        var longPredicate = graph.CreateUriNode(new Uri("http://data.countries/longitude"));
        var latPredicate = graph.CreateUriNode(new Uri("http://data.countries/latitude"));
        var typePredicate = graph.CreateUriNode(new Uri("http://data.countries/capitalType"));
        var popPredicate = graph.CreateUriNode(new Uri("http://data.countries/population"));
        // Обробка записів
        foreach (var record in records)
        {
            var countryUri = new Uri($"http://data.countries/{record.Name}");
            var countryNode = graph.CreateUriNode(countryUri);
            List<Triple> triples = new();
            // Name
            var nameNode = graph.CreateLiteralNode(record.Name);
            triples.Add(new Triple(countryNode, namePredicate, nameNode));

            // Capital 
            var capitalNode = graph.CreateLiteralNode(record.Capital);
            triples.Add(new Triple(countryNode, capitalPredicate, capitalNode));

            // Latitude
            var latNode = graph.CreateLiteralNode(record.Latitude.ToString(CultureInfo.InvariantCulture));
            triples.Add(new Triple(countryNode, latPredicate, latNode));

            // Longitude
            var longNode = graph.CreateLiteralNode(record.Longitude.ToString(CultureInfo.InvariantCulture));
            triples.Add(new Triple(countryNode, longPredicate, longNode));

            // Population
            var popNode = graph.CreateLiteralNode(record.Population.ToString());
            triples.Add(new Triple(countryNode, popPredicate, popNode));

            // CapitalType
            var typeNode = graph.CreateLiteralNode(record.CapitalType);
            triples.Add(new Triple(countryNode, typePredicate, typeNode));

            graph.Assert(triples);
        }
    }

    public string GetInfo(string countryName, string valueToSearch)
    {
        var graph = GetInstance().Graph;

        var countryUri = new Uri($"http://data.countries/{countryName}");

        var countryNode = graph.CreateUriNode(countryUri);

        var predicate = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSearch}"));

        var results = graph.GetTriplesWithSubjectPredicate(countryNode, predicate);

        var enumerable = results.ToList();
        if (enumerable.Any())
        {
            var capitalNode = enumerable.First().Object;

            if (capitalNode is ILiteralNode literalNode) return literalNode.Value;
        }

        throw new NotFoundException("Unable to find value");
    }
    
    public string LookupFrp(string countryName, string frpValueToWorkOut)
    {
        var graph = GetInstance().Graph;

        var countryUri = new Uri($"http://data.countries/{countryName}");

        var countryNode = graph.CreateUriNode(countryUri);

        var valueToLookUpFor = RulesIdentifier.GetRuleMapper(frpValueToWorkOut);
        
        var predicate = graph.CreateUriNode(new Uri($"http://data.countries/{valueToLookUpFor.Item2}"));

        var results = graph.GetTriplesWithSubjectPredicate(countryNode, predicate);

        var enumerable = results.ToList();
        string result = string.Empty;
        if (enumerable.Any())
        {
            var capitalNode = enumerable.First().Object;
            if (capitalNode is ILiteralNode literalNode)
                result = literalNode.Value;
        }
        else
        {
            throw new NotFoundException($"Unable to retrieve {valueToLookUpFor.Item2} from {countryName}.");
        }
        
        return valueToLookUpFor.Item1(int.Parse(result));

    }

    public bool UpdateInfo(string country, string valueToSearch, string newValue)
    {
        var graph = GetInstance().Graph;

        var countryUri = new Uri($"http://data.countries/{country}");

        var countryNode = graph.CreateUriNode(countryUri);

        var predicate = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSearch}"));

        var capitalPredicateLower = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSearch.ToLower()}"));

        var results = graph.GetTriplesWithSubjectPredicate(countryNode, predicate);

        var enumerable = results.ToList();
        enumerable.AddRange(graph.GetTriplesWithSubjectPredicate(countryNode, capitalPredicateLower));
        if (enumerable.Any())
        {
            var capitalNode = enumerable.First().Object;
            if (capitalNode is ILiteralNode)
            {
                graph.Retract(enumerable.First());
                graph.Assert(new Triple(countryNode, predicate, graph.CreateLiteralNode(newValue)));
                return true;
            }
        }

        throw new NotFoundException("Unable to find value");
    }

    public void SaveGraph(string filePath)
    {
        var graph = GetInstance().Graph;
        graph.SaveToFile(filePath);
    }
}