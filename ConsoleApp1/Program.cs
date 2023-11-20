using DatabaseRDF;

// Load data
DatabaseRdf.LoadDataFromDefaultCsvFile();

var continueRunning = true;
var db = DatabaseRdf.GetInstance();
while (continueRunning)
{
    // Get country  
    Console.Write("Enter country name: ");
    var country = Console.ReadLine() ?? string.Empty;
    if (country == string.Empty) continue;

    // Get field to lookup
    var lookupMappings = new Dictionary<string, string>()
    {
        { "1", "name" },
        { "2", "capital" },
        { "3", "latitude" },
        { "4", "longitude" },
        { "5", "population" },
        { "6", "capitalType" }
    };
    Console.WriteLine("Select lookup value:");
    foreach (var mapping in lookupMappings) Console.WriteLine($"{mapping.Key}. {mapping.Value}");

    int field;
    try
    {
        field = Convert.ToInt32(Console.ReadLine());
    }
    catch (Exception)
    {
        continue;
    }

    if (field < 0 || field > lookupMappings.Count)
    {
        Console.WriteLine("Invalid field");
        continue;
    }

    // Lookup value
    var result = db.GetInfo(country, lookupMappings.ElementAt(field - 1).Value);

    // Print result  
    Console.WriteLine(result);

    // See if user wants to update
    Console.Write("Update this value? (y/n) ");
    var update = Console.ReadLine() ?? "n";
    if (update != "y") continue;

    // Get new value
    Console.Write("Enter new value: ");
    var newValue = Console.ReadLine() ?? string.Empty;
    if (newValue == string.Empty)
    {
        Console.WriteLine("Invalid value");
        continue;
    }

    // Update graph
    var updated = db.UpdateInfo(country, lookupMappings.ElementAt(field - 1).Value, newValue);

    // Notify user
    if (updated)
        Console.WriteLine("Value updated successfully!");
    else
        Console.WriteLine("Unable to update value");

    // See if user wants to exit
    Console.Write("Lookup another value? (y/n) ");
    if (Console.ReadLine() != "y") continueRunning = false;
}

// Save data

Console.WriteLine("Do you want to save the rdf graph? (y/n)");
if (Console.ReadLine() == "y")
{
    Console.WriteLine("Specify file location (e.g. C:\\files\\:");
    var fileLocation = Console.ReadLine() ?? string.Empty;
    fileLocation += "graph.ttl";
    db.SaveGraph(fileLocation);
}