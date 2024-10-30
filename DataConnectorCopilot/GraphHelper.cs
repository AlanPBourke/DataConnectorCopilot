using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Serialization;
using System.Diagnostics;

namespace DataConnectorCopilot;

public static class GraphHelper
{
    private static GraphServiceClient? graphClient;
    private static HttpClient? httpClient;
    public static void Initialise(string tenantId, string clientId, string clientSecret)
    {
        // Create a credential that uses the client credentials
        // authorization flow
        var credential = new ClientSecretCredential(
            tenantId, clientId, clientSecret);

        // Create an HTTP client
        httpClient = GraphClientFactory.Create();

        // Create an auth provider
        var authProvider = new Microsoft.Kiota.Authentication.Azure.AzureIdentityAuthenticationProvider(
            credential, scopes: new[] { "https://graph.microsoft.com/.default" });

        //// Create a Graph client using the credential
        graphClient = new GraphServiceClient(httpClient, authProvider);

        var scopes = new[] { "https://graph.microsoft.com/.default" };
        graphClient = new(credential, scopes);
    }

    public static async Task<ExternalConnection?> CreateConnectionAsync(string id, string name, string? description)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");

        var newConnection = new ExternalConnection
        {
            Id = id,
            Name = name,
            Description = description,
        };

        return await graphClient.External.Connections.PostAsync(newConnection);
    }

    public static async Task<ExternalConnectionCollectionResponse?> GetExistingConnectionsAsync()
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");

        return await graphClient.External.Connections.GetAsync();
    }

    public static async Task DeleteConnectionAsync(string? connectionId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is required");

        Console.WriteLine("Doesn't work - do it via admin portal");
        await graphClient.External.Connections[connectionId].DeleteAsync();

    }

    public static async Task RegisterSchemaAsync2(string? connectionId, Schema schema)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");

        await graphClient.External
            .Connections[connectionId]
            .Schema
            .PatchAsync(schema);
    }

    public static async Task RegisterSchemaAsync(string? connectionId, Schema schema)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = httpClient ?? throw new MemberAccessException("httpClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is required");
        // Use the Graph SDK's request builder to generate the request URL
        var requestInfo = graphClient.External
            .Connections[connectionId]
            .Schema
            .ToGetRequestInformation();

        requestInfo.SetContentFromParsable(graphClient.RequestAdapter, "application/json", schema);

        // Convert the SDK request to an HttpRequestMessage
        var requestMessage = await graphClient.RequestAdapter
            .ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
        _ = requestMessage ?? throw new Exception("Could not create native HTTP request");
        requestMessage.Method = HttpMethod.Post;
        requestMessage.Headers.Add("Prefer", "respond-async");

        // Send the request
        var responseMessage = await httpClient.SendAsync(requestMessage) ??
            throw new Exception("No response returned from API");

        if (responseMessage.IsSuccessStatusCode)
        {
            // The operation ID is contained in the Location header returned
            // in the response
            var operationId = responseMessage.Headers.Location?.Segments.Last() ??
                throw new Exception("Could not get operation ID from Location header");
            await WaitForOperationToCompleteAsync(connectionId, operationId);
        }
        else
        {
            throw new ServiceException("Registering schema failed",
                responseMessage.Headers, (int)responseMessage.StatusCode);
        }
    }

    public static async Task AddOrUpdateItemAsync(string? connectionId, ExternalItem item)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is null");

        try
        {
            await graphClient.External
                .Connections[connectionId]
                .Items[item.Id]
                .PutAsync(item);
        }
        catch (ODataError ex)
        {
            Console.WriteLine($"{ex.Message} {ex.Error} {ex.InnerException}");
        }
    }

    public static async Task<Schema?> GetSchemaAsync(string? connectionId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is null");

        return await graphClient.External
            .Connections[connectionId]
            .Schema
            .GetAsync();
    }

    public static async Task DeleteItemAsync(string? connectionId, string? itemId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");
        _ = connectionId ?? throw new ArgumentException("connectionId is null");
        _ = itemId ?? throw new ArgumentException("itemId is null");

        await graphClient.External
            .Connections[connectionId]
            .Items[itemId]
            .DeleteAsync();
    }

    private static async Task WaitForOperationToCompleteAsync(string connectionId, string operationId)
    {
        _ = graphClient ?? throw new MemberAccessException("graphClient is null");

        do
        {
            var operation = await graphClient.External
                .Connections[connectionId]
                .Operations[operationId]
                .GetAsync();

            if (operation?.Status == ConnectionOperationStatus.Completed)
            {
                return;
            }
            else if (operation?.Status == ConnectionOperationStatus.Failed)
            {
                throw new ServiceException($"Schema operation failed: {operation?.Error?.Code} {operation?.Error?.Message}");
            }

            // Wait 5 seconds and check again
            await Task.Delay(5000);
        } while (true);
    }

    public static Schema GetSchema()
    {
        var schema = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties = new List<Property> {

                // Customer
                new() {
                    Name = "Name", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                    Labels = [Label.Title],
                },

                new() {
                    Name = "SalesAccountCode", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "Memo", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "AddressLine1", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "AddressLine2", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "AddressLine3", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "AddressLine4", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "PostCode", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "SalesOrderEmail", Type = PropertyType.String, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "TelephoneNumber", Type = PropertyType.String, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrdrContact", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderBalance", Type = PropertyType.Double, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderNumbers", Type = PropertyType.StringCollection, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderCustrefs", Type = PropertyType.StringCollection, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderDates", Type = PropertyType.DateTimeCollection, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderDueDates", Type = PropertyType.DateTimeCollection, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "OrderTotals", Type = PropertyType.DoubleCollection, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

/*
                new() {
                    Name = "customerTurnover", Type = PropertyType.Double, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },
 */                 
                // Orders
/*                
                new() { Name = "documentNumber", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false },

                new() {
                    Name = "customerReference", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "narrativeLine1", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "narrativeLine2", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "orderDate", Type = PropertyType.DateTime, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "salesOrder", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "totalOrderValue", Type = PropertyType.Double, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "orderMemo", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "dueDate", Type = PropertyType.DateTime, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                // Transactions
                new() {
                    Name = "lineStockReference", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "lineDescription", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "linePrice", Type = PropertyType.Double, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "lineQuantity", Type = PropertyType.Double, IsQueryable = false, IsSearchable = false, IsRetrievable = true, IsRefinable = false,
                },

                new() {
                    Name = "lineMemo", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false,
                },
*/
            },
        };

        return schema;
    }

    public static Schema GetSampleSchema()
    {

        var requestBody = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties = new List<Property>
    {   
        new Property
        {
            Name = "ticketTitle",
            Type = PropertyType.String,
            IsSearchable = true,
            IsRetrievable = true,
            Labels = new List<Label?>
            {
                Label.Title,
            },
        },
        new Property
        {
            Name = "priority",
            Type = PropertyType.String,
            IsQueryable = true,
            IsRetrievable = true,
            IsSearchable = false,
        },
        new Property
        {
            Name = "assignee",
            Type = PropertyType.String,
            IsRetrievable = true,
        },
    },
    };
    return requestBody;
    }

}
