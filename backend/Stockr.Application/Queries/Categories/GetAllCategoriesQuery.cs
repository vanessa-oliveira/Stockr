using MediatR;
using Stockr.Application.Models;
using Stockr.Domain.Entities;

namespace Stockr.Application.Queries.Categories;

public class GetAllCategoriesQuery : IRequest<IEnumerable<CategoryViewModel>>
{
}