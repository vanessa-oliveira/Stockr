using MediatR;

namespace Stockr.Application.Commands.Purchase;

public class DeletePurchaseCommand : IRequest<Unit>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
}