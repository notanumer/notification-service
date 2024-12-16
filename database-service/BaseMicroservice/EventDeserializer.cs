using System.Text;
using System.Text.Json;
using DatabaseService.Models.Rabbit;
using RabbitMQ.Client.Events;

namespace BaseMicroservice;

public static class EventDeserializer
{
    public static Event? Deserialize(BasicDeliverEventArgs args)
    {
        var body = args.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var ev = JsonSerializer.Deserialize<Event>(message);

        return ev;
    }
}