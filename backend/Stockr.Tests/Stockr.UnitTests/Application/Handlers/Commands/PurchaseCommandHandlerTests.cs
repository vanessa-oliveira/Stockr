using AutoFixture;
using NSubstitute;
using Stockr.Application.Commands.Purchase;
using Stockr.Application.Handlers.Commands;
using Stockr.Infrastructure.Repositories;

namespace Stockr.UnitTests.Application.Handlers.Commands;

public class PurchaseCommandHandlerTests
{
    private PurchaseCommandHandler _sut;
    private IPurchaseRepository _purchaseRepository;
    private IPurchaseItemRepository _purchaseItemRepository;
    private IInventoryRepository _inventoryRepository;
    private IInventoryMovementRepository _inventoryMovementRepository;
    private Fixture _fixture;

    public PurchaseCommandHandlerTests()
    {
        _purchaseRepository = Substitute.For<IPurchaseRepository>();
        _purchaseItemRepository = Substitute.For<IPurchaseItemRepository>();
        _inventoryRepository = Substitute.For<IInventoryRepository>();
        _inventoryMovementRepository = Substitute.For<IInventoryMovementRepository>();
        _fixture = new Fixture();
    }

    [Fact]
    public async Task deve_ser_possivel_registrar_uma_nova_compra()
    {
        //Arrange
        var cmd = _fixture.Create<CreatePurchaseCommand>();

        //Act
        await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        
    }
}