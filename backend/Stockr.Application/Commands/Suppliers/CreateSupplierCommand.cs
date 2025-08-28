using MediatR;

namespace Stockr.Application.Commands.Suppliers;

public class CreateSupplierCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Phone { get; set; }
}