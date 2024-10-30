using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Graph.Models.ODataErrors;
using Newtonsoft.Json;
using DataConnectorCopilot.Models;
using Microsoft.Graph.Models;
using System.Text;

namespace DataConnectorCopilot;

class DataConnectorCopilot
{
    static async Task Main(string[] args)
    {
        // Change as appropriate.
        var clientId = Environment.GetEnvironmentVariable(@"DC_COPILOT_CLIENTID", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var tenantId = Environment.GetEnvironmentVariable(@"DC_COPILOT_TENANTID", EnvironmentVariableTarget.Machine) ?? string.Empty;
        var clientSecret = Environment.GetEnvironmentVariable(@"DC_COPILOT_CLIENTSECRET", EnvironmentVariableTarget.Machine) ?? string.Empty;

        InitialiseGraph(tenantId, clientId, clientSecret);

        ExternalConnection? currentConnection = null;
        int choice = -1;

        //var odc = new OperaDataConnectorHelper();
        //var SalesCustsJson = await odc.GetDataAsJson(@"SalesAccounts");
        //var SalesCusts = JsonConvert.DeserializeObject<OperaSalesAccounts>(SalesCustsJson);
        // var json = await OperaDataConnectorHelper.GetSalesOrderData();
        // var json = await OperaDataConnectorHelper.GetStockData();
        //Console.WriteLine(json);
       // var e = await OperaToGraphMapper.GetExternalItems();

        while (choice != 0)
        {
            Console.WriteLine($"Current connection: {(currentConnection == null ? "NONE" : currentConnection.Name)}\n");
            Console.WriteLine("Please choose one of the following options:");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Create a connection");
            Console.WriteLine("2. Select an existing connection");
            Console.WriteLine("3. Delete current connection");
            Console.WriteLine("4. Register schema for current connection");
            Console.WriteLine("5. View schema for current connection");
            Console.WriteLine("6. Push updated items to current connection");
            Console.WriteLine("7. Push ALL items to current connection");
            Console.Write("Selection: ");

            try
            {
                choice = int.Parse(Console.ReadLine() ?? string.Empty);
            }
            catch (FormatException)
            {
                // Set to invalid value
                choice = -1;
            }

            switch (choice)
            {
                case 0:
                    // Exit the program
                    Console.WriteLine("Exiting ...");
                    break;
                case 1:
                    currentConnection = await CreateConnectionAsync();
                    break;
                case 2:
                    currentConnection = await SelectExistingConnectionAsync();
                    break;
                case 3:
                    await DeleteCurrentConnectionAsync(currentConnection);
                    currentConnection = null;
                    break;
                case 4:
                    if (currentConnection is not null)
                    {
                        await RegisterSchemaAsync(currentConnection);
                    }
                    else
                    {
                        Console.WriteLine(@"No current connection!");
                    }
                    break;
                case 5:
                    if (currentConnection is not null)
                    {
                        await GetSchemaAsync(currentConnection);
                    }
                    else
                    {
                        Console.WriteLine(@"No current connection!");
                    }
                    break;
                case 6:
                    if (currentConnection is not null)
                        await UpdateItemsFromDatabaseAsync(currentConnection, true, tenantId);
                    else
                    {
                        Console.WriteLine(@"No current connection!");
                    }
                    break;
                case 7:
                    //await UpdateItemsFromDatabaseAsync(false, settings.TenantId);
                    break;
                default:
                    Console.WriteLine("Invalid choice! Please try again.");
                    break;
            }
        }

    }

    static async Task<ExternalConnection?> CreateConnectionAsync()
    {
        var connectionId = PromptForInput(
            "Enter a unique ID for the new connection (3-32 characters)", true) ?? "ConnectionId";
        var connectionName = PromptForInput(
            "Enter a name for the new connection", true) ?? "ConnectionName";
        var connectionDescription = PromptForInput(
            "Enter a description for the new connection", false);

        try
        {
            // Create the connection
            var connection = await GraphHelper.CreateConnectionAsync(
                connectionId, connectionName, connectionDescription);
            Console.WriteLine($"New connection created - Name: {connection?.Name}, Id: {connection?.Id}");
            return connection;
        }
        catch (ODataError odataError)
        {
            Console.WriteLine($"Error creating connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
            return null;
        }
    }

    static string? PromptForInput(string prompt, bool valueRequired)
    {
        string? response;

        do
        {
            Console.WriteLine($"{prompt}:");
            response = Console.ReadLine();
            if (valueRequired && string.IsNullOrEmpty(response))
            {
                Console.WriteLine("You must provide a value");
            }
        } while (valueRequired && string.IsNullOrEmpty(response));

        return response;
    }

    static DateTime GetLastUploadTime()
    {
        if (File.Exists("lastuploadtime.bin"))
        {
            return DateTime.Parse(
                File.ReadAllText("lastuploadtime.bin")).ToUniversalTime();
        }

        return DateTime.MinValue;
    }

    static void SaveLastUploadTime(DateTime uploadTime)
    {
        File.WriteAllText("lastuploadtime.bin", uploadTime.ToString("u"));
    }

    static public void InitialiseGraph(string tenantId, string clientId, string clientSecret)
    {
        GraphHelper.Initialise(tenantId, clientId, clientSecret);
    }

    static async Task<ExternalConnection?> SelectExistingConnectionAsync()
    {
        Console.WriteLine("Getting existing connections...");
        try
        {
            var response = await GraphHelper.GetExistingConnectionsAsync();
            var connections = response?.Value ?? new List<ExternalConnection>();
            if (connections.Count <= 0)
            {
                Console.WriteLine("No connections exist. Please create a new connection");
                return null;
            }

            // Display connections
            Console.WriteLine("Choose one of the following connections:");
            var menuNumber = 1;
            foreach (var connection in connections)
            {
                Console.WriteLine($"{menuNumber++}. {connection.Name}");
            }

            ExternalConnection? selection = null;

            do
            {
                try
                {
                    Console.Write("Selection: ");
                    var choice = int.Parse(Console.ReadLine() ?? string.Empty);
                    if (choice > 0 && choice <= connections.Count)
                    {
                        selection = connections[choice - 1];
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice.");
                    }
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid choice.");
                }
            } while (selection == null);

            return selection;
        }
        catch (ODataError odataError)
        {
            Console.WriteLine($"Error getting connections: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
            return null;
        }
    }

    static async Task DeleteCurrentConnectionAsync(ExternalConnection? connection)
    {
        if (connection == null)
        {
            Console.WriteLine(
                "No connection selected. Please create a new connection or select an existing connection.");
            return;
        }

        try
        {
            await GraphHelper.DeleteConnectionAsync(connection.Id);
            Console.WriteLine($"{connection.Name} deleted successfully.");
        }
        catch (ODataError odataError)
        {
            Console.WriteLine($"Error deleting connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    static async Task RegisterSchemaAsync(ExternalConnection currentConnection)
    {
        if (currentConnection == null)
        {
            Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
            return;
        }

        Console.WriteLine("Registering schema, this may take a moment...");

        try
        {
            
            // Create the schema
            var schema = GraphHelper.GetSchema();
            //var schema = GraphHelper.GetSampleSchema();
            await GraphHelper.RegisterSchemaAsync2(currentConnection.Id, schema);
            Console.WriteLine("Schema registered successfully");
        }
        catch (ServiceException serviceException)
        {
            Console.WriteLine($"Error registering schema: {serviceException.ResponseStatusCode} {serviceException.Message}");
        }
        catch (ODataError odataError)
        {
            Console.WriteLine($"Error registering schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering schema: {ex.Message}");
        }
    }

    static async Task GetSchemaAsync(ExternalConnection currentConnection)
    {
        if (currentConnection == null)
        {
            Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
            return;
        }

        try
        {
            var schema = await GraphHelper.GetSchemaAsync(currentConnection.Id);
            if (schema is not null)
            {
                var json = JsonConvert.SerializeObject(schema);
                File.WriteAllText(@"c:\temp\schema.json", json);
                Console.WriteLine("Schema written.");
            }
            // Console.WriteLine(JsonSerializer.Serialize(schema));

        }
        catch (ODataError odataError)
        {
            Console.WriteLine($"Error getting schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    static async Task UpdateItemsFromDatabaseAsync(ExternalConnection currentConnection, bool uploadModifiedOnly, string? tenantId)
    {
        var success = true;
        var newUploadTime = DateTime.UtcNow;
        var items = await OperaToGraphMapper.GetExternalItems();
        File.WriteAllText(@"c:\temp\items.json", JsonConvert.SerializeObject(items, Formatting.Indented));

        foreach (var item in items)
        {
            try
            {
                Console.WriteLine($"Uploading {item.Id} ...");
                await GraphHelper.AddOrUpdateItemAsync(currentConnection.Id, item);
            }
            catch (ODataError odataError)
            {
                success = false;
                Console.WriteLine("FAILED");
                Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
            }
        }
        
        if (success)
        {
            SaveLastUploadTime(newUploadTime);
        }
    }
}
