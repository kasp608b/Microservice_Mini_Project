using EasyNetQ;
using SharedModels;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        IBus bus;

        public MessagePublisher(string connectionString)
        {
            bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public void PublishCreditStandingChangedMessage(int customerId, bool newCreditStanding)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = customerId,
                NewCreditStanding = newCreditStanding
            };
            Console.WriteLine(message + "message published");
            bus.PubSub.Publish(message);

        }


        public void PublishOrderStatusChangedMessage(int? customerId, List<OrderLineDto> orderLines, string topic)
        {
            var message = new OrderStatusChangedMessage
            {
                CustomerId = customerId,
                OrderLines = orderLines
            };
            Console.WriteLine(message + "message published" + topic);
            bus.PubSub.Publish(message, topic);
        }

        public void PublishOrderRejectedMessage(OrderDto order, string messageText)
        {
            var message = new OrderRejectedMessage
            {
                orderDto = order,
                message = messageText
            };
            Console.WriteLine(message + "message published");
            bus.PubSub.Publish(message);
        }

    }
}
