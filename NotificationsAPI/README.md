# FCG Notifications API — Fase 4

Worker .NET que consome `UserCreatedEvent` e `PaymentProcessedEvent` e registra as notificações
(e-mail de boas-vindas, confirmação ou rejeição de compra) no log.

Na Fase 4 ele substitui a Azure Function da Fase 3, para que toda a solução fique em uma única nuvem (AWS/EKS).

- Credenciais do RabbitMQ: Secret `fcg-secrets`; o worker não sobe sem elas.
- Logs: `kubectl logs -n fcg deploy/notifications-api -f`
- Testes: `dotnet test NotificationsAPI.slnx`
- Pipeline: [.github/workflows/notifications-api.yml](../.github/workflows/notifications-api.yml)