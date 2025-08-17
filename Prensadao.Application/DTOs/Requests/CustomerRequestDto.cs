namespace Prensadao.Application.DTOs.Requests
{
    public class CustomerRequestDto
    {
        public int? CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Phone { get; set; }
        public string Street { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ReferencePoint { get; set; } = string.Empty;
        public int Cep { get; set; }
    }
}
