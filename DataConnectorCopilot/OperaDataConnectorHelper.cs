using System.Diagnostics;
using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
namespace DataConnectorCopilot;

public static class OperaDataConnectorHelper
{
    public static async Task<string> GetDataAsJson(string query)
    {
        var httpClient = new HttpClient(new HttpClientHandler()
        {
            UseDefaultCredentials = true
        });

        string s = string.Empty;

        try
        {
            s = await httpClient.GetStringAsync($"http://localhost:8081/odata/C/0/Z/{query}?");
            var f = Path.Combine(@"c:\temp\", $"{query}.json" );
            File.Delete(f);
            if (s.Length > 0)
            {
                File.WriteAllText(f, s);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);  
        }

        return s;
    }

    public static async Task<string> GetSalesOrderData()
    {
        var httpClient = new HttpClient(new HttpClientHandler()
        {
            UseDefaultCredentials = true
        });

        string json = string.Empty;

        var salesaccountsfields = @"Name,SalesAccountCode,Memo,AddressLine1,AddressLine2,AddressLine3,AddressLine4,PostCode,SalesOrderEmail,";
        salesaccountsfields += @"TelephoneNumber,OrdrContact,OrderBalance,RecordId";
        var sopheadersfields = @"DocumentNumber,SalesOrder,OrderDate,CustomerReference,NarrativeLine1,NarrativeLine2,ExcludingVat,Vat,Memo,DueDate,RaisedBy";
        var soptransactionsfields = @"DocumentNumber,StockReference,ItemPrice,Memo,LineQuantity,GoodsValue,CostPrice,VatAmount,Description,";
        var stockitemfields = @"StockReference";

        // DataConnector only allows an expansion depth of 2, so return json down as far as customer->order->transaction 
        var stockitems = $"StockItem($select={stockitemfields};$filter=RecordStatus eq 1)";         // NB not StockItems with an S
        var soptransactions = $"SOPTransactions($select={soptransactionsfields};$filter=RecordStatus eq 1 and Status eq 'A')";
        var sopheaders = $"SOPHeaders($select={sopheadersfields};$filter=Status eq 'O' and RecordStatus eq 1;$Expand={soptransactions};$orderby=OrderDate desc)";

        var query = $"SalesAccounts?Select={salesaccountsfields}";
        query += $"&$Expand={sopheaders}";
        query += @"&$Filter=RecordStatus eq 1 and dormant eq false and stop eq false";

        Debug.WriteLine(query);

        try
        {
            json = await httpClient.GetStringAsync($"http://localhost:8081/odata/C/0/Z/{query}");
            TextCopy.ClipboardService.SetText(json);

        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return json;
    }

    public static string GetSalesOrderDataFromFile()
    {
        return File.ReadAllText(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SampleData"), "salesorders.json"));
    }

    public static async Task<string> GetStockData()
    {
        var httpClient = new HttpClient(new HttpClientHandler()
        {
            UseDefaultCredentials = true
        });

        string json = string.Empty;

        var stockprofilesfields = @"StockProfileCode,Description,QDecimalPlaces,PDecimalPlaces";
        var stockprofiles = $"StockProfile($select={stockprofilesfields};$filter=RecordStatus eq 1)";
        var stockitemfields = @"StockReference,Description,SearchReference1,SearchReference2,SellingPrice,CostPrice,Memo";

        var stockitems = $"StockItems?select={stockitemfields}&$Expand={stockprofiles}&$filter=RecordStatus eq 1 and dormant eq false";

        var query = $"{stockitems}";

        Debug.WriteLine(query);

        try
        {
            json = await httpClient.GetStringAsync($"http://localhost:8081/odata/C/0/Z/{query}");
            TextCopy.ClipboardService.SetText(json);

        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return json;
    }
    
}
