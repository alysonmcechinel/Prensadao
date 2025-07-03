using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application
{
    public static class RabbitMqConstants
    {
        public static class Exchanges
        {
            public const string OrderExchange = "prensado.order";
        }

        public static class RoutingKeys
        {
            public const string OrderCreatedRoutingKey = "prensado.order.created";
        }

        public static class Queues
        {
            public const string OrderCozinhaQueue = "prensado.orders.cozinha";
            public const string OrderNotifyQueue = "prensado.orders.notify";
        }
    }
}
