using DatabaseRDF;
using DatabaseRDF.Exceptions;

DatabaseRdf.LoadDataFromDefaultCsvFile();

var db = DatabaseRdf.GetInstance();
while (true)
{
    bool isFrp = false;
    Console.Write("Enter country name: ");
    var country = Console.ReadLine() ?? string.Empty;
    if (country == string.Empty) continue;

    var lookupMappings = new Dictionary<string, string>
    {
        { "1", "name" },
        { "2", "capital" },
        { "3", "latitude" },
        { "4", "longitude" },
        { "5", "population" },
        { "6", "capitalType" },
    };

    var lookUpFrp = new Dictionary<string, string>
    {
        {"101", "isCountryBig"}
        
    };
        
    Console.WriteLine("Select lookup value:");
    foreach (var mapping in lookupMappings) Console.WriteLine($"{mapping.Key}. {mapping.Value}");
    foreach (var mapping in lookUpFrp) Console.WriteLine($"{mapping.Key}. {mapping.Value}");

    int field;
    try
    {
        field = Convert.ToInt32(Console.ReadLine());
    }
    catch (Exception)
    {
        Console.WriteLine("Invalid input");
        continue;
    }


    try
    {
        string result;
        if (field < 100)
        {
            result = db.GetInfo(country, lookupMappings[field.ToString()]);
        }
        else
        {
            result = db.LookupFrp(country, lookUpFrp[field.ToString()]);
            isFrp = true;
        }
        Console.WriteLine(result);
        
    }
    catch (NotFoundException exception)
    {
        Console.WriteLine(exception.Message);
        continue;
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine("Invalid field");
        continue;
    }
    catch (Exception)
    {
        Console.WriteLine("Something went wrong");
        continue;
    }

    if (isFrp)
    {
        Console.Write("Lookup another value? (y/n) ");
        if (Console.ReadLine() != "y") break;
        continue;
    }

    Console.Write("Update this value? (y/n) ");
    var update = Console.ReadLine() ?? "n";
    if (update != "y") continue;

    Console.Write("Enter new value: ");
    var newValue = Console.ReadLine() ?? string.Empty;
    if (newValue == string.Empty)
    {
        Console.WriteLine("Invalid value");
        continue;
    }

    var updated = db.UpdateInfo(country, lookupMappings.ElementAt(field - 1).Value, newValue);

    Console.WriteLine(updated ? "Value updated successfully!" : "Unable to update value");

    Console.Write("Lookup another value? (y/n) ");
    if (Console.ReadLine() != "y") break;
}


Console.WriteLine("Do you want to save the rdf graph? (y/n)");
if (Console.ReadLine() == "y")
{
    Console.WriteLine("Specify file location (e.g. C:\\files\\:");
    var fileLocation = Console.ReadLine() ?? string.Empty;
    fileLocation += "graph.ttl";
    db.SaveGraph(fileLocation);
    Console.WriteLine($"Graph saved to {fileLocation}");
}