using System.ComponentModel.DataAnnotations;

namespace Prensadao.Application.DTOs.Requests
{
    public class CustomerRequestDto
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public long Phone { get; set; }
        public string Street { get; set; }
        public string District { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string ReferencePoint { get; set; }
        public int Cep { get; set; }
    }
}
