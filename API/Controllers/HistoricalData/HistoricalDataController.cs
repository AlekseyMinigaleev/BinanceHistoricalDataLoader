using API.Controllers.HistoricalData.Actions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HistoricalData
{
    [Route("api/historical-data")]
    public class HistoricalDataController(IMediator mediator) : BaseController(mediator)
    {
        [HttpPost("load")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Load(
            [FromBody] Load.LoadQuery parameters,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(parameters, cancellationToken);

            return Ok(id);
        }
    }
}