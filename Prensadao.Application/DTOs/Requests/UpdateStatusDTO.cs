using Prensadao.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.DTOs.Requests;

public class UpdateStatusDTO
{
    public int OrderId { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
}
