namespace Prensadao.Application
{
    //TODO: deixar aqui por enquanto depois decidir o lugar certo
    public static class RabbitMqConstants
    {
        public static class Exchanges
        {
            public const string OrderExchange = "prensado.order";
            public const string OrderStatusChangedExchange = "prensado.order-status-changed";
            public const string DeadLetterExchange = "prensado.dead-letter";
        }

        public static class RoutingKeys
        {
            public const string OrderCreatedRoutingKey = "prensado.order.created";
            public const string OrderCozinhaDeadLetterRoutingKey = "prensado.orders.cozinha.dead-letter";
            public const string OrderStatusMessageDeadLetterRoutingKey = "prensado.order-status-messages.dead-letter";
        }

        public static class Queues
        {
            public const string OrderCozinhaQueue = "prensado.orders.cozinha";
            public const string OrderStatusMessageQueue = "prensado.order-status-messages";
            public const string OrderCozinhaDeadLetterQueue = "prensado.orders.cozinha.dlq";
            public const string OrderStatusMessageDeadLetterQueue = "prensado.order-status-messages.dlq";
        }
    }
}
