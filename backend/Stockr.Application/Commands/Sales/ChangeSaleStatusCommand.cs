using MediatR;
using Stockr.Domain.Enums;

namespace Stockr.Application.Commands.Sales;

public class ChangeSaleStatusCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public string NewStatus { get; set; }
}