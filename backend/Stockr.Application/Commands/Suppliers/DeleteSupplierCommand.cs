using MediatR;

namespace Stockr.Application.Commands.Suppliers;

public class DeleteSupplierCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
}