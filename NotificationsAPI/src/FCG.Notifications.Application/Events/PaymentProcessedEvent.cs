namespace FCG.Payments.Application.Events;

/// <summary>Status pode ser "Approved" ou "Rejected".</summary>
public record PaymentProcessedEvent(
    Guid OrderId,
    int UserId,
    int GameId,
    string GameName,
    string UserEmail,
    string Status,
    DateTime ProcessedAt);
