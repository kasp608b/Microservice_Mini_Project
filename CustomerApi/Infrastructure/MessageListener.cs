using CustomerApi.Data;
using EasyNetQ;
using SharedModels;

namespace CustomerApi.Infrastructure
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

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {

                Console.WriteLine("Started Listening on " + connectionString + " ");

                // * shipped
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiHkShipped",
                    HandleOrderShipped, x => x.WithTopic("shipped"));

                // * credit standing changed
                bus.PubSub.Subscribe<CreditStandingChangedMessage>("customerApiHkCreditStandingChanged",
                    HandleCreditStandingChanged);

                // * order rejected
                bus.PubSub.Subscribe<OrderRejectedMessage>("customerApiHkOrderRejected",
                    HandleOrderRejected);

                // * order compleated
                bus.PubSub.Subscribe<OrderStatusChangedMessage>("customerApiHkCompleted",
                   HandleOrderCompleted, x => x.WithTopic("completed"));

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }

        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message)
        {
            Console.WriteLine("Handle order completed called");
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var emailService = services.GetService<IEmailService>();
                var customerRepo = services.GetService<ICustomerRepository>();
                var productServiceGateway = services.GetService<IServiceGateway<ProductDto>>();


                if (message.CustomerId != null && customerRepo != null && emailService != null && productServiceGateway != null)
                {
                    var customer = customerRepo.Get((int)message.CustomerId);

                    decimal totalPrice = 0;

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Dear {customer.CompanyName}");
                    sb.AppendLine($"Order with order id number: {message.OrderLines[0].OrderId} has been completed.");
                    sb.AppendLine($"The following is a list of the products and the number of ordered products");
                    foreach (OrderLineDto orderline in message.OrderLines)
                    {
                        var product = productServiceGateway.Get(orderline.ProductId);
                        sb.AppendLine($"Product name: {product.Name}, Price: {product.Price}, Number of ordered items: {orderline.NoOfItems}");

                        totalPrice += product.Price * orderline.NoOfItems;
                    }

                    sb.AppendLine($"Total price: {totalPrice}kr.");
                    sb.AppendLine($"The ordered products will be shipped shortly");
                    sb.AppendLine($"Have a nice day");

                    emailService.SendEmail(customer.Email, "Order completed", sb.ToString());

                }

            }
        }

        private void HandleOrderRejected(OrderRejectedMessage message)
        {
            Console.WriteLine("Handle order rejected called");
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var emailService = services.GetService<IEmailService>();
                var customerRepo = services.GetService<ICustomerRepository>();
                var productServiceGateway = services.GetService<IServiceGateway<ProductDto>>();


                if (customerRepo != null && emailService != null && productServiceGateway != null)
                {
                    var customer = customerRepo.Get((int)message.orderDto.CustomerId);

                    decimal totalPrice = 0;

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Dear {customer.CompanyName}");
                    sb.AppendLine($"Order with order id number: {message.orderDto.OrderId} has been Rejected.");
                    sb.AppendLine($"The following is a list of the products and the number of ordered products");
                    foreach (OrderLineDto orderline in message.orderDto.Orderlines)
                    {
                        var product = productServiceGateway.Get(orderline.ProductId);
                        sb.AppendLine($"Product name: {product.Name}, Price: {product.Price}, Number of ordered items: {orderline.NoOfItems}");

                        totalPrice += product.Price * orderline.NoOfItems;
                    }

                    sb.AppendLine($"Total price: {totalPrice}kr.");
                    sb.AppendLine($"The order was rejected because: {message.message}");

                    emailService.SendEmail(customer.Email, "Order rejected", sb.ToString());

                }

            }
        }

        private void HandleCreditStandingChanged(CreditStandingChangedMessage message)
        {
            Console.WriteLine("Handle credit status changed called");
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepo = services.GetService<ICustomerRepository>();


                if (customerRepo != null)
                {
                    var customer = customerRepo.Get((int)message.CustomerId);

                    customer.CreditStanding = message.NewCreditStanding;

                    customerRepo.Edit(customer);

                }

            }
        }

        private void HandleOrderShipped(OrderStatusChangedMessage message)
        {
            Console.WriteLine("Handle order shipped called");
            using (var scope = provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var emailService = services.GetService<IEmailService>();
                var customerRepo = services.GetService<ICustomerRepository>();
                var productServiceGateway = services.GetService<IServiceGateway<ProductDto>>();


                if (message.CustomerId != null && customerRepo != null && emailService != null && productServiceGateway != null)
                {
                    var customer = customerRepo.Get((int)message.CustomerId);

                    customer.CreditStanding = false;

                    customerRepo.Edit(customer);

                    decimal totalPrice = 0;

                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine($"Dear {customer.CompanyName}");
                    sb.AppendLine($"Order with order id number: {message.OrderLines[0].OrderId} has been shipped.");
                    sb.AppendLine($"The following is a list of the products and the number of ordered products");
                    foreach (OrderLineDto orderline in message.OrderLines)
                    {
                        var product = productServiceGateway.Get(orderline.ProductId);
                        sb.AppendLine($"Product name: {product.Name}, Price: {product.Price}, Number of ordered items: {orderline.NoOfItems}");

                        totalPrice += product.Price * orderline.NoOfItems;
                    }

                    sb.AppendLine($"Total price: {totalPrice}kr.");
                    sb.AppendLine($"Until this invoice is paid you will be unable to make further orders");
                    sb.AppendLine($"Please pay");

                    emailService.SendEmail(customer.Email, "Order shipped", sb.ToString());

                }

            }
        }


    }
}
