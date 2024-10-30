using DataConnectorCopilot.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Newtonsoft.Json;

namespace DataConnectorCopilot;

public static class OperaToGraphMapper
{
    public static async Task<List<ExternalItem>> GetExternalItems()
    {
        var items = new List<ExternalItem>();

        Console.WriteLine("Getting Sales Order data ...");

        // For issue report, mock test data. Normally comes from an OData source.
        //var salesOrderCustomersJson = await OperaDataConnectorHelper.GetSalesOrderData();
        var salesOrderCustomersJson = OperaDataConnectorHelper.GetSalesOrderDataFromFile();
        var salesOrderCustomers = JsonConvert.DeserializeObject<OperaSalesOrderCustomers>(salesOrderCustomersJson);

        if (salesOrderCustomers is null || salesOrderCustomers.CustomersWithOrders is null)
        {
            return items;
        }

        foreach (var customerWithOrders in salesOrderCustomers.CustomersWithOrders.Where(o => o.SOPHeaders!.Count >0))
        {
            var orderNumbers = new List<string>();
            var orderCustrefs = new List<string>();
            var orderDates = new List<DateTime>();
            var orderDueDates = new List<DateTime>();
            var orderTotals = new List<double>();

            foreach (var salesOrder in customerWithOrders.SOPHeaders!)
            {
                orderNumbers.Add(salesOrder.SalesOrder);
                orderCustrefs.Add(salesOrder.CustomerReference);
                orderDates.Add(salesOrder.OrderDate);
                orderDueDates.Add(salesOrder.DueDate);
                orderTotals.Add(salesOrder.ExcludingVat + salesOrder.Vat);
            }

            var item = new ExternalItem
            {
                Id = customerWithOrders.SalesAccountCode,
                Acl = new() { new() { Type = AclType.Everyone, Value = "everyone", AccessType = AccessType.Grant } },
                Content = new ExternalItemContent
                {
                    Type = ExternalItemContentType.Text,
                    Value = $"{customerWithOrders.Name} {customerWithOrders.SalesAccountCode}",
                },
                Properties = new Properties
                {
                    // https://learn.microsoft.com/en-ie/graph/api/externalconnectors-externalconnection-put-items?view=graph-rest-1.0&tabs=http
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "Name", customerWithOrders.Name },
                        { "SalesAccountCode", customerWithOrders.SalesAccountCode },
                        { "Memo@odata.type", "Collection(String)" },
                        { "Memo", customerWithOrders.Memo },
                        { "AddressLine1", customerWithOrders.AddressLine1 },
                        { "AddressLine2", customerWithOrders.AddressLine2 },
                        { "AddressLine3", customerWithOrders.AddressLine3 },
                        { "AddressLine4", customerWithOrders.AddressLine4 },
                        { "PostCode", customerWithOrders.PostCode },
                        { "SalesOrderEmail", customerWithOrders.SalesOrderEmail },
                        { "TelephoneNumber", customerWithOrders.TelephoneNumber },
                        { "OrdrContact", customerWithOrders.OrdrContact },
                        { "OrdrBalance", Convert.ToDouble(customerWithOrders.OrderBalance) },
                        { "OrderNumbers@odata.type", "Collection(String)" },
                        { "OrderNumbers", orderNumbers },
                        { "OrderCustrefs@odata.type", "Collection(String)" },
                        { "OrderCustrefs", orderCustrefs },
                        { "OrderDates@odata.type", "Collection(DateTimeOffset)" },
                        { "OrderDates", orderDates },
                        { "OrderDueDates@odata.type", "Collection(DateTimeOffset)" },
                        { "OrderDueDates", orderDueDates },
                        { "OrderTotals@odata.type", "Collection(Double)" },
                        { "OrderTotals", orderTotals },

                    }
                }
            };

            items.Add(item);
        }
        
        return items;
    }

}
