using AutoFixture;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Domain.Enums;
using Stockr.Infrastructure.Repositories;

namespace Stockr.UnitTests.Application.Services;

public class PurchaseInventoryServiceTests
{
    private readonly PurchaseInventoryService _sut;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly ILogger<PurchaseInventoryService> _logger;
    private readonly Fixture _fixture;

    public PurchaseInventoryServiceTests()
    {
        _inventoryRepository = Substitute.For<IInventoryRepository>();
        _inventoryMovementRepository = Substitute.For<IInventoryMovementRepository>();
        _logger = Substitute.For<ILogger<PurchaseInventoryService>>();
        _fixture = CreateFixture();
        _sut = new PurchaseInventoryService(_inventoryRepository, _inventoryMovementRepository, _logger);
    }

    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private PurchaseItem CreatePurchaseItem(Guid? id = null, Guid? productId = null, int? quantity = null, decimal? unitPrice = null)
    {
        var item = _fixture.Create<PurchaseItem>();

        if (id.HasValue) item.Id = id.Value;
        if (productId.HasValue) item.ProductId = productId.Value;
        if (quantity.HasValue) item.Quantity = quantity.Value;
        if (unitPrice.HasValue) item.UnitPrice = unitPrice.Value;

        return item;
    }

    private Inventory CreateInventory(Guid? productId = null, int? currentStock = null)
    {
        var inventory = _fixture.Create<Inventory>();

        if (productId.HasValue) inventory.ProductId = productId.Value;
        if (currentStock.HasValue) inventory.CurrentStock = currentStock.Value;

        return inventory;
    }

    [Fact]
    public async Task deve_processar_estoque_durante_acao_de_compra()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var movementDate = _fixture.Create<DateTime>();
        var purchaseItems = _fixture.CreateMany<PurchaseItem>(2).ToList();
        var inventories = _fixture.CreateMany<Inventory>(2).ToList();

        // Configura os ProductIds para garantir correspondência
        inventories[0].ProductId = purchaseItems[0].ProductId;
        inventories[1].ProductId = purchaseItems[1].ProductId;

        var initialStock1 = inventories[0].CurrentStock;
        var initialStock2 = inventories[1].CurrentStock;

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.ProcessPurchaseInventoryAsync(purchaseId, purchaseItems, movementDate);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count == 2 &&
            movements.All(m => m.Direction == MovementDirection.In) &&
            movements.All(m => m.PurchaseId == purchaseId) &&
            movements.All(m => m.MovementDate == movementDate)));

        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count == 2 &&
            invs.First(i => i.ProductId == purchaseItems[0].ProductId).CurrentStock == initialStock1 + purchaseItems[0].Quantity &&
            invs.First(i => i.ProductId == purchaseItems[1].ProductId).CurrentStock == initialStock2 + purchaseItems[1].Quantity));
    }

    [Fact]
    public async Task deve_processar_atualizacao_de_estoque_com_novos_itens()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var productId1 = _fixture.Create<Guid>();
        var productId2 = _fixture.Create<Guid>();
        var existingItemId = _fixture.Create<Guid>();

        var existingItems = new List<PurchaseItem>
        {
            CreatePurchaseItem(existingItemId, productId1, 5)
        };

        var newItems = new List<PurchaseItem>
        {
            CreatePurchaseItem(existingItemId, productId1, 8),
            CreatePurchaseItem(productId: productId2, quantity: 10)
        };

        var inventories = new List<Inventory>
        {
            CreateInventory(productId1),
            CreateInventory(productId2)
        };

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.ProcessInventoryUpdateAsync(purchaseId, existingItems, newItems, userId);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count > 0));
        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count > 0));
    }

    [Fact]
    public async Task deve_reverter_estoque_ao_excluir_compra()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var purchaseItems = _fixture.CreateMany<PurchaseItem>(2).ToList();
        var inventories = _fixture.CreateMany<Inventory>(2).ToList();

        // Configura os ProductIds e estoque inicial
        inventories[0].ProductId = purchaseItems[0].ProductId;
        inventories[1].ProductId = purchaseItems[1].ProductId;
        inventories[0].CurrentStock = purchaseItems[0].Quantity + 15;
        inventories[1].CurrentStock = purchaseItems[1].Quantity + 10;

        var expectedStock1 = inventories[0].CurrentStock - purchaseItems[0].Quantity;
        var expectedStock2 = inventories[1].CurrentStock - purchaseItems[1].Quantity;

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.RevertPurchaseInventoryAsync(purchaseId, purchaseItems, userId);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count == 2 &&
            movements.All(m => m.Direction == MovementDirection.Out) &&
            movements.All(m => m.PurchaseId == purchaseId) &&
            movements.All(m => m.UserId == userId)));

        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count == 2 &&
            invs.First(i => i.ProductId == purchaseItems[0].ProductId).CurrentStock == expectedStock1 &&
            invs.First(i => i.ProductId == purchaseItems[1].ProductId).CurrentStock == expectedStock2));
    }

    [Fact]
    public async Task deve_atualizar_quantidade_quando_item_for_modificado()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var itemId = _fixture.Create<Guid>();
        var productId = _fixture.Create<Guid>();

        var existingItem = CreatePurchaseItem(itemId, productId, 10);
        var newItem = CreatePurchaseItem(itemId, productId, 15);
        var inventory = CreateInventory(productId, 30);

        var existingItems = new List<PurchaseItem> { existingItem };
        var newItems = new List<PurchaseItem> { newItem };
        var inventories = new List<Inventory> { inventory };

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.ProcessInventoryUpdateAsync(purchaseId, existingItems, newItems, userId);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count > 0 &&
            movements.Any(m => m.Direction == MovementDirection.In)));
        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count > 0));
    }

    [Fact]
    public async Task deve_reduzir_estoque_quando_quantidade_for_diminuida()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var itemId = _fixture.Create<Guid>();
        var existingItem = CreatePurchaseItem(itemId, quantity: 20);
        var newItem = CreatePurchaseItem(itemId, existingItem.ProductId, 12);
        var inventory = CreateInventory(existingItem.ProductId, 50);

        var existingItems = new List<PurchaseItem> { existingItem };
        var newItems = new List<PurchaseItem> { newItem };
        var inventories = new List<Inventory> { inventory };

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.ProcessInventoryUpdateAsync(purchaseId, existingItems, newItems, userId);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count > 0 &&
            movements.Any(m => m.Direction == MovementDirection.Out)));
        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count > 0));
    }

    [Fact]
    public async Task deve_remover_item_completamente_do_estoque()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var userId = _fixture.Create<Guid>();
        var existingItem = CreatePurchaseItem(quantity: 10);
        var inventory = CreateInventory(existingItem.ProductId, 25);

        var existingItems = new List<PurchaseItem> { existingItem };
        var newItems = new List<PurchaseItem>();
        var inventories = new List<Inventory> { inventory };

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(inventories);

        //Act
        await _sut.ProcessInventoryUpdateAsync(purchaseId, existingItems, newItems, userId);

        //Assert
        await _inventoryMovementRepository.Received(1).AddRangeAsync(Arg.Is<List<InventoryMovement>>(movements =>
            movements.Count == 1 &&
            movements[0].Direction == MovementDirection.Out &&
            movements[0].Quantity == 10));

        await _inventoryRepository.Received(1).UpdateRangeAsync(Arg.Is<List<Inventory>>(invs =>
            invs.Count == 1 &&
            invs[0].CurrentStock == 15));
    }

    [Fact]
    public async Task nao_deve_processar_produto_sem_inventario_configurado()
    {
        //Arrange
        var purchaseId = _fixture.Create<Guid>();
        var movementDate = _fixture.Create<DateTime>();
        var purchaseItem = _fixture.Create<PurchaseItem>();
        var purchaseItems = new List<PurchaseItem> { purchaseItem };

        _inventoryRepository.GetByProductIdsAsync(Arg.Any<List<Guid>>()).Returns(new List<Inventory>());

        //Act
        await _sut.ProcessPurchaseInventoryAsync(purchaseId, purchaseItems, movementDate);

        //Assert
        await _inventoryMovementRepository.DidNotReceive().AddRangeAsync(Arg.Any<List<InventoryMovement>>());
        await _inventoryRepository.DidNotReceive().UpdateRangeAsync(Arg.Any<List<Inventory>>());
    }
}