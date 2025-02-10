using FluentValidation.Results;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public class BaseController(IMediator mediator) : ControllerBase
    {
        protected readonly IMediator _mediator = mediator;

        protected async Task ValidateAndChangeModelStateAsync<T>(
            IValidator<T> validator,
            T instance,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await validator
                .ValidateAsync(instance, cancellationToken);

            if (!validationResult.IsValid)
                ChangeModelState(validationResult.Errors);
        }

        private void ChangeModelState(IEnumerable<ValidationFailure> errors)
        {
            foreach (var item in errors)
                ModelState.AddModelError(
                    item.ErrorCode,
                    item.ErrorMessage);
        }
    }
}