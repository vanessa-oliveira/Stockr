using MediatR;

namespace Stockr.Application.Commands.Sales;

public class DeleteSaleCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}