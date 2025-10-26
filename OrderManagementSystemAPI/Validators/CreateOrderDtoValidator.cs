using FluentValidation;
using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}