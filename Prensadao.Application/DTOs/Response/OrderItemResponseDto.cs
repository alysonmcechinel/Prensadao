namespace Prensadao.Application.Models.Response
{
    public class OrderItemResponseDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ProductValue { get; set; }
        public string ProductName { get; set; }
    }
}
