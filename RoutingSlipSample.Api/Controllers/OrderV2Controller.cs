using MassTransit;
using Microsoft.AspNetCore.Mvc;
using RoutingSlipSample.Api.Models;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderV2Controller : ControllerBase
{


    private readonly ILogger<OrderV2Controller> _logger;
    private readonly IRequestClient<SubmitOrderV2> _client;

    public OrderV2Controller(ILogger<OrderV2Controller> logger, IRequestClient<SubmitOrderV2> client)
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
        try
        {
          
            Response response = await _client.GetResponse<OrderSubmissionAcceptedV2>(new {  order.OrderId });

            return response switch
            {
                (_, OrderSubmissionAcceptedV2 accepted) => Accepted(new SubmitOrderResponseModel()
                {
                    OrderId = order.OrderId
                }),
                _ => BadRequest()
            };
        }
        catch (Exception e)
        {

        }

        return BadRequest();
    }
}