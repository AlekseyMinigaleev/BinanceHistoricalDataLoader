using API.Controllers.HistoricalData.Actions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.HistoricalData
{
    [Route("api/historical-data")]
    public class HistoricalDataController(
        IMediator mediator)
        : BaseController(mediator)
    {
        [HttpPost("load")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Load.LoadResponse>> Load(
            [FromBody] Load.LoadQuery parameters,
            [FromServices] IValidator<Load.LoadQuery> validator,
            CancellationToken cancellationToken)
        {
            await ValidateAndChangeModelStateAsync(
                validator,
                parameters,
                cancellationToken);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _mediator.Send(parameters, cancellationToken);

            return Ok(response);
        }

        [HttpPost("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Status.StatusResponse>> Status(
            [FromQuery] Guid jobId,
            CancellationToken cancellationToken)
        {
            var request = new Status.StatusQuery { JobId = jobId };

            var response = await _mediator.Send(request, cancellationToken);

            if (response is null)
                return NotFound();

            return Ok(response);
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("report")]
        public async Task<ActionResult<Report.CandlestickVM[]>> Report(
            [FromQuery] Guid reportId,
            CancellationToken cancellationToken)
        {
            var request = new Report.ReportQuery { Id = reportId };
            var result = await _mediator.Send(request, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}