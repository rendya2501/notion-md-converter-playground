

services.AddSingleton<EventBus>();
services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<EventBus>());

// --- OutputDirectoryChangedEvent.cs ---
public record OutputDirectoryChangedEvent(string OutputDirectory);

// --- IEventPublisher.cs ---
public interface IEventPublisher
{
    void Publish<T>(T eventData);
}

// --- IEventSubscriber.cs ---
public interface IEventSubscriber<T>
{
    void OnEvent(T eventData);
}

// --- EventBus.cs ---
public class EventBus : IEventPublisher
{
    private readonly List<object> _subscribers = new();

    public void Subscribe<T>(IEventSubscriber<T> subscriber)
    {
        _subscribers.Add(subscriber);
    }

    public void Publish<T>(T eventData)
    {
        foreach (var subscriber in _subscribers.OfType<IEventSubscriber<T>>())
        {
            subscriber.OnEvent(eventData);
        }
    }
}

