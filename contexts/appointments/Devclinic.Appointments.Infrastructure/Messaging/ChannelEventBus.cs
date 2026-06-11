using System.Threading.Channels;

using Devclinic.Appointments.Application.Abstractions;

namespace Devclinic.Appointments.Infrastructure.Messaging;

public sealed class ChannelEventBus : IEventBus
{
    private readonly Channel<object> _channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
    {
        SingleReader = false, SingleWriter = false
    });

    public ValueTask<object> ReadAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAsync(ct);

    public IAsyncEnumerable<object> ReadAllAsync(CancellationToken ct = default) =>
        _channel.Reader.ReadAllAsync(ct);

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class
    {
        await _channel.Writer.WriteAsync(@event, ct);
    }
}