using MediatR;

namespace Stockr.Application.Commands.Customers;

public class DeleteCustomerCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}