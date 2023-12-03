RDF Database

This project implements an RDF database for storing and querying country data.
Usage

The main class is DatabaseRdf which provides methods for loading data, querying, and saving the RDF graph.

To get started:

    Call DatabaseRdf.LoadDataFromDefaultCsvFile() to load sample country data
    Get an instance of the database with var db = DatabaseRdf.GetInstance()
    Lookup a value for a country using db.GetInfo(countryName, fieldName)
    Update a value using db.UpdateInfo(countryName, fieldName, newValue)
    Save the graph to a file with db.SaveGraph(filePath)

A console application is included that demonstrates:

    Loading sample country data
    Looking up values for a country
    Updating values
    Saving the graph to a file

The user is prompted to enter a country name and select a field to lookup. The value is displayed and the user can choose to update it. After updating multiple values, the graph can be saved to a file.
Implementation

The database uses an in-memory VDS RDF graph to store the RDF data.

Country data is loaded from a sample CSV file into the graph using predicates for each field. Literal nodes are created for field values.

SPARQL queries against the graph subjects are used to lookup and update values.
Data

The sample CountriesData.csv contains country name, capital, latitude, longitude, population, and capital type.

Additional data files can be added and loaded by modifying LoadDataFromDefaultCsvFile().