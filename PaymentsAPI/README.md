# FCG Payments API — Fase 4

Worker .NET que consome `OrderPlacedEvent` (fila `fcg.order.placed`), simula a aprovação do pagamento
(taxa configurável em `Payment__TaxaAprovacaoPercent`, padrão 80%) e publica `PaymentProcessedEvent`.

- Credenciais do RabbitMQ: Secret `fcg-secrets` (`RABBITMQ_USER`/`RABBITMQ_PASSWORD`); o worker não sobe sem elas.
- Testes: `dotnet test PaymentsAPI.slnx`
- Pipeline: [.github/workflows/payments-api.yml](../.github/workflows/payments-api.yml)
- Execução local: [docs/GUIA-LOCAL.md](../docs/GUIA-LOCAL.md)