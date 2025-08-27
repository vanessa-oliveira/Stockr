using MediatR;
using Stockr.Application.Models;

namespace Stockr.Application.Queries.Products;

public class GetProductByIdQuery : IRequest<ProductViewModel?>
{
    public Guid Id { get; set; }
}