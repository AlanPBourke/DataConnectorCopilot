using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConnectorCopilot.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class OperaStockItems
{
    [JsonProperty("@odata.context")]
    public string odatacontext { get; set; } = string.Empty;

    [JsonProperty("value")]
    public List<StockItem>? StockItems { get; set; }
}

public class StockProfile
{
    [JsonProperty("StockProfileCode")]
    public string StockProfileCode { get; set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("QDecimalPlaces")]
    public int? QDecimalPlaces { get; set; } 

    [JsonProperty("PDecimalPlaces")]
    public int? PDecimalPlaces { get; set; }
}

public class StockItem
{
    [JsonProperty("StockReference")]
    public string StockReference { get; set; } = string.Empty;

    [JsonProperty("SearchReference1")]
    public string SearchReference1 { get; set; } = string.Empty;

    [JsonProperty("SearchReference2")]
    public string SearchReference2 { get; set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("Memo")]
    public string Memo { get; set; } = string.Empty;

    [JsonProperty("SellingPrice")]
    public double? SellingPrice { get; set; }

    [JsonProperty("CostPrice")]
    public double? CostPrice { get; set; }

    [JsonProperty("StockProfile")]
    public StockProfile? StockProfile { get; set; } 
}


