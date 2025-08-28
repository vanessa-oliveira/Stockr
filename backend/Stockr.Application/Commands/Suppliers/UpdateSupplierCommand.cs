using MediatR;

namespace Stockr.Application.Commands.Suppliers;

public class UpdateSupplierCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
}