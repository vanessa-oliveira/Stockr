using MediatR;

namespace Stockr.Application.Commands.Customers;

public class CreateCustomerCommand : IRequest<Unit>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? CPF { get; set; }
    public string? CNPJ { get; set; }
}