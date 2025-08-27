using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Products;

public class GetAllProductsQuery : IRequest<IEnumerable<ProductViewModel>>
{
}