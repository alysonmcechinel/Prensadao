namespace Prensadao.Domain.ReadModels;

public class TopProductSalesModels
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}
