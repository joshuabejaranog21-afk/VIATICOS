namespace OneCardExpenseValidator.API.Models;

public class ParsedTicketData
{
    public string? Vendor { get; set; }
    public DateTime? TicketDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public List<ParsedTicketItem> Items { get; set; } = new();
    public string? RawText { get; set; }
}

public class ParsedTicketItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class MatchedTicketItem
{
    public ParsedTicketItem OriginalItem { get; set; } = new();
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool? IsDeductible { get; set; }
    public int MatchScore { get; set; }
    public bool IsMatched => ProductId.HasValue;
}
