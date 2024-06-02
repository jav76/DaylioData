# DaylioData
Package for accessing Daylio data from exported CSV files.

## Usage

The `DaylioData` class is the main class for accessing Daylio data. It has a single constructor that takes a string path to the CSV file.
Once initialized, deserialized CSV data can be accessed through `DaylioData.DataRepo`, and certain summary statistics can be accessed through `DaylioData.DataSummary`.

```csharp
DaylioData daylioData = new DaylioData("path_to_your_file.csv");
string summary = daylioData.DataSummary.GetSummary();
```

Nuget: https://www.nuget.org/packages/DaylioData/
