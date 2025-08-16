using System.ComponentModel;

namespace Prensadao.Domain.Enums
{
    public enum OrderStatusEnum
    {
        [Description("Criado")]
        Criado = 0,
        [Description("Em preparacao")]
        EmPreparacao = 1,
        [Description("Pronto")]
        Pronto = 2,
        [Description("Saiu para entrega")]
        SaiuParaEntrega = 3,
        [Description("Finalizado")]
        Finalizado = 4,
        [Description("Cancelado")]
        Cancelado = 5,
        [Description("Error")]
        Error = 99
    }
}
