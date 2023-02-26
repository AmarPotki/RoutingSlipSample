using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RoutingSlipSample.Api.Models;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<OrderController> _logger;
        private readonly IRequestClient<SubmitOrder> _client;

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> client)
        {
            _logger = logger;
            _client = client;
        }

        /// <summary>
        /// Submits an order
        /// <param name="order">The order model</param>
        /// <response code="200">The order has been completed</response>
        /// <response code="202">The order has been accepted but not yet completed</response>
        /// <response code="400">The order could not be completed</response>
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(OrderModel order)
        {
            Response response = await _client.GetResponse<OrderSubmissionAccepted>(new
            {
                order.OrderId,
            });

            return response switch
            {
                (_, OrderSubmissionAccepted accepted) => Accepted(new SubmitOrderResponseModel()
                {
                    OrderId = order.OrderId
                }),
                _ => BadRequest()
            };
        }
    }
}