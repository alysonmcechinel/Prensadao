namespace Prensadao.Application.DTOs.Requests
{
    public class ProductRequestDto
    {
        public int? ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ProductEnabledDto
    {
        public int ProductId { get; set; }
        public bool Enabled { get; set; }
    }
}