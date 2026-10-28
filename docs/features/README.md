# Features Técnicas

Este diretório contém decisões e desenhos de funcionalidades que ainda serão implementadas ou cuja evolução precisa permanecer separada da documentação do comportamento atual.

| Feature | Estado | Responsabilidade |
| --- | --- | --- |
| [`kitchen-admission-control.md`](kitchen-admission-control.md) | Confirmada, com configuração do MVP ainda aberta | Capacidade da cozinha, fila por chegada e horário de aceitação de pedidos |
| [`rabbitmq-message-reliability.md`](rabbitmq-message-reliability.md) | MVP implementado | Dead-letter com uma DLQ por fluxo e sem retry automático |
| [`order-status-outbox.md`](order-status-outbox.md) | Confirmada, desenho técnico ainda aberto | Atomicidade entre alteração do pedido e publicação do evento de status |
| [`k6-performance-baseline.md`](k6-performance-baseline.md) | Confirmada, pronta para planejamento | Linha de base local e detecção de regressão de desempenho na jornada de pedidos |

Comportamentos efetivamente disponíveis continuam documentados em `docs/FEATURES.md`. Uma feature planejada não deve ser apresentada ali como concluída.
