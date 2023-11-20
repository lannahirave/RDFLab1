using System.Globalization;
using System.Reflection;
using CsvHelper;
using VDS.RDF;

namespace DatabaseRDF;

public class DatabaseRdf
{
    private DatabaseRdf()
    {
    }

    private static DatabaseRdf? _instance;

    private static readonly object Lock = new();

    public static DatabaseRdf GetInstance()
    {
        if (_instance is null)
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new DatabaseRdf();
                    _instance.Graph = new Graph();
                }
            }

        return _instance;
    }

    private IGraph Graph { get; set; }


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
            var latNode = graph.CreateLiteralNode(record.Latitude.ToString());
            triples.Add(new Triple(countryNode, latPredicate, latNode));

            // Longitude
            var longNode = graph.CreateLiteralNode(record.Longitude.ToString());
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

    public string GetInfo(string countryName, string valueToSeach)
    {
        var graph = GetInstance().Graph;

        var countryUri = new Uri($"http://data.countries/{countryName}");

        var countryNode = graph.CreateUriNode(countryUri);

        var predicate = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSeach}"));

        var capitalPredicateLower = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSeach.ToLower()}"));

        var results = graph.GetTriplesWithSubjectPredicate(countryNode, predicate);

        var enumerable = results.ToList();
        enumerable.AddRange(graph.GetTriplesWithSubjectPredicate(countryNode, capitalPredicateLower));
        if (enumerable.Any())
        {
            var capitalNode = enumerable.First().Object;

            if (capitalNode is ILiteralNode literalNode) return literalNode.Value;
        }

        return "Not found";
    }

    public bool UpdateInfo(string country, string valueToSeach, string newValue)
    {
        var graph = GetInstance().Graph;

        var countryUri = new Uri($"http://data.countries/{country}");

        var countryNode = graph.CreateUriNode(countryUri);

        var predicate = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSeach}"));

        var capitalPredicateLower = graph.CreateUriNode(new Uri($"http://data.countries/{valueToSeach.ToLower()}"));

        var results = graph.GetTriplesWithSubjectPredicate(countryNode, predicate);

        var enumerable = results.ToList();
        enumerable.AddRange(graph.GetTriplesWithSubjectPredicate(countryNode, capitalPredicateLower));
        if (enumerable.Any())
        {
            var capitalNode = enumerable.First().Object;

            if (capitalNode is ILiteralNode literalNode)
            {
                graph.Retract(enumerable.First());
                graph.Assert(new Triple(countryNode, predicate, graph.CreateLiteralNode(newValue)));
                return true;
            }
        }

        return false;
    }

    public void SaveGraph(string filePath)
    {
        var graph = GetInstance().Graph;
        graph.SaveToFile(filePath);
    }
}