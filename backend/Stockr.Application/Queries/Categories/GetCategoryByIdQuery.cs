using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Entities;

namespace Stockr.Application.Queries.Categories;

public class GetCategoryByIdQuery : IRequest<CategoryViewModel?>
{
    public Guid Id { get; set; }
}