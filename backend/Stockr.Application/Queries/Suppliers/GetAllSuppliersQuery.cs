using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Suppliers;

public class GetAllSuppliersQuery : IRequest<IEnumerable<SupplierViewModel>>
{
}