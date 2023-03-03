using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        public RestClient client;

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
            client = new RestClient("https://localhost:44396");
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        /// <summary>
        /// Update order status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            var modifiedOrder = repository.Get(id);

            if (modifiedOrder == null)
            {
                return NotFound();
            }

            modifiedOrder.Status = order.Status;


            repository.Edit(modifiedOrder);
            return new NoContentResult();
        }


        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            foreach (var orderline in order.Orderlines)
            {


                // Call ProductApi to get the product ordered
                // You may need to change the port number in the BaseUrl below
                // before you can run the request.
                //RestClient c = new RestClient("https://localhost:5001/products/");
                var request = new RestRequest("/products/" + orderline.ProductId.ToString());
                var response = client.GetAsync<Product>(request);
                response.Wait();
                var orderedProduct = response.Result;



                if (orderline.NoOfItems <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                {
                    // reduce the number of items in stock for the ordered product,
                    // and create a new order.
                    orderedProduct.ItemsReserved += orderline.NoOfItems;
                    var updateRequest = new RestRequest("/products/" + orderedProduct.ProductId.ToString());
                    updateRequest.AddJsonBody(orderedProduct);
                    var updateResponse = client.PutAsync(updateRequest);
                    updateResponse.Wait();

                    if (!updateResponse.IsCompletedSuccessfully)
                    {
                        return NoContent();
                    }
                }
                else {
                    return BadRequest("Items in stock is less than the number of items ordered");
                }

            }

            var newOrder = repository.Add(order);
            return CreatedAtRoute("GetOrder",
                new { id = newOrder.OrderId }, newOrder);

            // If the order could not be created, "return no content".
            //return NoContent();
        }

    }
}
