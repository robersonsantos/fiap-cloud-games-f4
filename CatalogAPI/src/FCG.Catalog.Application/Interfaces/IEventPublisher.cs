namespace FCG.Catalog.Application.Interfaces;

public interface IEventPublisher
{
    Task PublicarAsync<T>(T evento) where T : class;
}
