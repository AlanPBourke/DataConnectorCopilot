using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConnectorCopilot.Models;

public class OperaSalesOrderCustomers 
{
    [JsonProperty("@odata.context")]
    public string? odatacontext { get; set; }

    [JsonProperty("value")]
    public List<SalesOrderCustomer>? CustomersWithOrders { get; set; }
}

public class SOPHeader
{
    [JsonProperty("DocumentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonProperty("CustomerReference")]
    public string CustomerReference { get; set; } = string.Empty;

    [JsonProperty("NarrativeLine1")]
    public string NarrativeLine1 { get; set; } = string.Empty;

    [JsonProperty("NarrativeLine2")]
    public string NarrativeLine2 { get; set; } = string.Empty;

    [JsonProperty("OrderDate")]
    public DateTime OrderDate { get; set; }

    [JsonProperty("SalesOrder")]
    public string SalesOrder { get; set; } = string.Empty;

    [JsonProperty("ExcludingVat")]
    public double ExcludingVat { get; set; }

    [JsonProperty("Vat")]
    public double Vat { get; set; }

    [JsonProperty("Memo")]
    public string Memo { get; set; } = string.Empty;

    [JsonProperty("DueDate")]
    public DateTime DueDate { get; set; }

    [JsonProperty("RaisedBy")]
    public string RaisedBy { get; set; } = string.Empty;

    [JsonProperty("SOPTransactions")]
    public List<SOPTransaction>? SOPTransactions { get; set; } 
}

public class SOPTransaction
{
    [JsonProperty("DocumentNumber")]
    public string DocumentNumber { get; set; } = string.Empty;

    [JsonProperty("StockReference")]
    public string StockReference { get; set; } = string.Empty;

    [JsonProperty("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("ItemPrice")]
    public int? ItemPrice { get; set; }

    [JsonProperty("Memo")]
    public string Memo { get; set; } = string.Empty;

    [JsonProperty("LineQuantity")]
    public double LineQuantity { get; set; }

    [JsonProperty("GoodsValue")]
    public double? GoodsValue { get; set; }

    [JsonProperty("VatAmount")]
    public double? VatAmount { get; set; }

    [JsonProperty("CostPrice")]
    public double? CostPrice { get; set; }
}

public class SalesOrderCustomer
{
    [JsonProperty("RecordId")]
    public int RecordId { get; set; } 

    [JsonProperty("Name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("SalesAccountCode")]
    public string SalesAccountCode { get; set; } = string.Empty;

    [JsonProperty("Memo")]
    public string Memo { get; set; } = string.Empty;

    [JsonProperty("AddressLine1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [JsonProperty("AddressLine2")]
    public string AddressLine2 { get; set; } = string.Empty;

    [JsonProperty("AddressLine3")]
    public string AddressLine3 { get; set; } = string.Empty;

    [JsonProperty("AddressLine4")]
    public string AddressLine4 { get; set; } = string.Empty;

    [JsonProperty("PostCode")]
    public string PostCode { get; set; } = string.Empty;

    [JsonProperty("SalesOrderEmail")]
    public string SalesOrderEmail { get; set; } = string.Empty;

    [JsonProperty("TelephoneNumber")]
    public string TelephoneNumber { get; set; } = string.Empty;

    [JsonProperty("OrdrContact")]
    public string OrdrContact { get; set; } = string.Empty;

    [JsonProperty("OrderBalance")]
    public decimal OrderBalance { get; set; }

    [JsonProperty("SOPHeaders")]
    public List<SOPHeader>? SOPHeaders { get; set; } 
}

