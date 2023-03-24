using EasyNetQ;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider provider;
        string connectionString;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider provider, string connectionString)
        {
            this.provider = provider;
            this.connectionString = connectionString;
        }

        public async Task StartAsync()
        {
            //Implement the functionality for the method to try to connect again if the connection fails.
            var connectionEstablished = false;

            
                using (var bus = RabbitHutch.CreateBus(connectionString))
                {
                
                while (!connectionEstablished)
            {


                    Console.WriteLine("Started Listening on " + connectionString + " ");

                    var subscriptionResult = bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("productApiHkCompleted",
                        HandleOrderCompleted, x => x.WithTopic("completed")).AsTask();

                    // Add code to subscribe to other OrderStatusChanged events:
                    // * cancelled
                    bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkCancelled",
                        HandleOrderCancelled, x => x.WithTopic("cancelled"));
                    // * shipped
                    bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiHkShipped",
                        HandleOrderShipped, x => x.WithTopic("shipped"));
                    // * paid
                    // Implement an event handler for each of these events.
                    // Be careful that each subscribe has a unique subscription id
                    // (this is the first parameter to the Subscribe method). If they
                    // get the same subscription id, they will listen on the same
                    // queue.

                    await subscriptionResult.WaitAsync(CancellationToken.None);
                    connectionEstablished = subscriptionResult.Status == TaskStatus.RanToCompletion;
                    if (!connectionEstablished) Thread.Sleep(1000);
                }

                Console.WriteLine("connectionEstablished= " + connectionEstablished);
                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                    {
                        Monitor.Wait(this);
                    }

                    
                
            }

        }

        private void HandleOrderShipped(OrderStatusChangedMessage message)
        {
            Console.WriteLine("Handle order shipped called");
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                //Remove the reservartion items of ordered product (should be a single transaction).
                //Remove the Items form the stock of the ordered product (should be a single transaction).

                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.NoOfItems;
                    product.ItemsInStock -= orderLine.NoOfItems;
                    productRepos.Edit(product);
                }
            }
        }

        private void HandleOrderCancelled(OrderStatusChangedMessage message)
        {
            Console.WriteLine("Handle order cancelled called");
            using (var scope = provider.CreateScope())
            {

                Console.WriteLine("Revieces Order Cancelled Message");
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                //Remove the reservartion items of ordered product (should be a single transaction).
               
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved -= orderLine.NoOfItems;
                    productRepos.Edit(product);
                }
            }

        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            Console.WriteLine("Handle order completed called");
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (var orderLine in message.OrderLines)
                {
                    var product = productRepos.Get(orderLine.ProductId);
                    product.ItemsReserved += orderLine.NoOfItems;
                    productRepos.Edit(product);
                }
            }
        }

    }
}
