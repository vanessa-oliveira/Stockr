using MediatR;

namespace Stockr.Application.Commands.Customers;

public class UpdateCustomerCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? CPF { get; set; }
    public string? CNPJ { get; set; }
}