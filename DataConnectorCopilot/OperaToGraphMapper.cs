using DataConnectorCopilot.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

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
            //var orderDates = new List<DateTime>();
            //var orderDueDates = new List<DateTime>();
            //var orderTotals = new List<double>();
            var orderDates = new List<string>();
            var orderDueDates = new List<string>();
            var orderTotals = new List<string>();

            foreach (var salesOrder in customerWithOrders.SOPHeaders!)
            {
                orderNumbers.Add(salesOrder.SalesOrder);
                orderCustrefs.Add(salesOrder.CustomerReference);
                //orderDates.Add(salesOrder.OrderDate);
                //orderDueDates.Add(salesOrder.DueDate);
                //orderTotals.Add(salesOrder.ExcludingVat + salesOrder.Vat);
                //Debug.WriteLine(salesOrder.OrderDate);
                orderDates.Add(salesOrder.OrderDate.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
                orderDueDates.Add(salesOrder.DueDate.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
                //orderDates.Add(salesOrder.OrderDate);
                //orderDueDates.Add(salesOrder.DueDate);
                orderTotals.Add(Math.Round(salesOrder.ExcludingVat + salesOrder.Vat, 2).ToString("F"));
            }

            var item = new ExternalItem
            {
                Id = customerWithOrders.SalesAccountCode,
                Acl = new() { new() { Type = AclType.Everyone, Value = "everyone", AccessType = AccessType.Grant } },
                Content = new ExternalItemContent
                {
                    Type = ExternalItemContentType.Text,
                    Value = $"{customerWithOrders.Name}",
                },
                Properties = new Properties
                {
                    // https://learn.microsoft.com/en-ie/graph/api/externalconnectors-externalconnection-put-items?view=graph-rest-1.0&tabs=http
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "Name", customerWithOrders.Name },
                        { "SalesAccountCode", customerWithOrders.SalesAccountCode },
                        { "Memo", customerWithOrders.Memo ?? string.Empty },
                        { "AddressLine1", customerWithOrders.AddressLine1 ?? string.Empty },
                        { "AddressLine2", customerWithOrders.AddressLine2 ?? string.Empty },
                        { "AddressLine3", customerWithOrders.AddressLine3 ?? string.Empty },
                        { "AddressLine4", customerWithOrders.AddressLine4 ?? string.Empty },
                        { "PostCode", customerWithOrders.PostCode ?? string.Empty },
                        { "SalesOrderEmail", customerWithOrders.SalesOrderEmail ?? string.Empty },
                        { "TelephoneNumber", customerWithOrders.TelephoneNumber ?? string.Empty },
                        { "OrdrContact", customerWithOrders.OrdrContact ?? string.Empty },
                        { "OrderBalance", customerWithOrders.OrderBalance },
                        { "OrderNumbers@odata.type", "Collection(String)" },
                        { "OrderNumbers", orderNumbers.ToArray() },
                        { "OrderCustrefs@odata.type", "Collection(String)" },
                        { "OrderCustrefs", orderCustrefs.ToArray() },
                        { "OrderTotals@odata.type", "Collection(String)" },
                        { "OrderTotals", orderTotals.ToArray() },
                        { "OrderDates@odata.type", "Collection(String)" },
                        { "OrderDates", orderDates.ToArray() },
                        { "OrderDueDates@odata.type", "Collection(String)" },
                        { "OrderDueDates", orderDueDates.ToArray() },

                    }
                }
            };

            items.Add(item);
        }
        
        return items;
    }

}
