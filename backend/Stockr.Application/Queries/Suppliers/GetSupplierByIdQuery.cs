using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Suppliers;

public class GetSupplierByIdQuery : IRequest<SupplierViewModel?>
{
    public Guid Id { get; set; }
}