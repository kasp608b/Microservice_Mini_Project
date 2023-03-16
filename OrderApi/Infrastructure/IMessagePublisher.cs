using SharedModels;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishCreditStandingChangedMessage(int customerId, bool newCreditStanding);
        void PublishOrderStatusChangedMessage(int? customerId,
            List<OrderLineDto> orderLines, string topic);

        void PublishOrderRejectedMessage(OrderDto order, string messageText);
    }
}
