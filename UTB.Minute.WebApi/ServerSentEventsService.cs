using System.Collections.Concurrent;
using System.Net.ServerSentEvents;
using System.Threading.Channels;
using UTB.Minute.Contracts;

namespace UTB.Minute.WebApi;

public class ServerSentEventsService
{
    private readonly ConcurrentDictionary<Guid, Channel<SseItem<CanteenStateDto>>> subscribers = new();

    public async Task WriteAsync(CanteenStateDto state)
    {
        
        foreach (Channel<SseItem<CanteenStateDto>> channel in subscribers.Values)
        {
            SseItem<CanteenStateDto> sseItem = new(state);
            await channel.Writer.WriteAsync(sseItem);
        }
    }

    public IAsyncEnumerable<SseItem<CanteenStateDto>> InitAndGetStream(CanteenStateDto initData, CancellationToken ct)
    {
        var clientId = Guid.NewGuid();
        var clientChannel = Channel.CreateBounded<SseItem<CanteenStateDto>>(new BoundedChannelOptions(20) { FullMode = BoundedChannelFullMode.DropOldest });

        ct.Register(() => subscribers.TryRemove(clientId, out _));
        subscribers.TryAdd(clientId, clientChannel);
        
        clientChannel.Writer.TryWrite(new SseItem<CanteenStateDto>(initData));

        return clientChannel.Reader.ReadAllAsync(ct);
    }
}

