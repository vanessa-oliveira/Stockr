using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Stockr.Application.Commands.Purchase;
using Stockr.Application.Handlers.Commands;
using Stockr.Application.Services;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;

namespace Stockr.UnitTests.Application.Handlers.Commands;

public class PurchaseCommandHandlerTests
{
    private PurchaseCommandHandler _sut;
    private IPurchaseRepository _purchaseRepository;
    private IPurchaseItemRepository _purchaseItemRepository;
    private IPurchaseInventoryService _purchaseInventoryService;
    private IPurchaseItemService _purchaseItemService;
    private ITenantService _tenantService;
    private ILogger<PurchaseCommandHandler> _logger;
    private Fixture _fixture;

    public PurchaseCommandHandlerTests()
    {
        _purchaseRepository = Substitute.For<IPurchaseRepository>();
        _purchaseItemRepository = Substitute.For<IPurchaseItemRepository>();
        _purchaseInventoryService =  Substitute.For<IPurchaseInventoryService>();
        _purchaseItemService = Substitute.For<IPurchaseItemService>();
        _tenantService = Substitute.For<ITenantService>();
        _logger = Substitute.For<ILogger<PurchaseCommandHandler>>();
        _fixture = CreateFixture();
        _sut = new PurchaseCommandHandler(_purchaseRepository, _purchaseItemRepository, _purchaseInventoryService, _purchaseItemService, _tenantService, _logger);
    }
    
    private static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    #region CreatePurchase

    [Fact]
    public async Task deve_ser_possivel_registrar_uma_nova_compra()
    {
        //Arrange
        var cmd = _fixture.Create<CreatePurchaseCommand>();
        cmd.PurchaseItems = _fixture.CreateMany<CreatePurchaseItemCommand>().ToList();
        var tenantId = _fixture.Create<Guid>();
        var purchaseItems = _fixture.CreateMany<PurchaseItem>().ToList();

        _tenantService.GetCurrentTenantId().Returns(tenantId);
        _purchaseItemService.CalculateTotalAmount(cmd.PurchaseItems).Returns(_fixture.Create<decimal>());
        _purchaseItemService.CreatePurchaseItemsAsync(Arg.Any<Guid>(), cmd.PurchaseItems).Returns(purchaseItems);

        //Act
        await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await _purchaseRepository.Received(1).AddAsync(Arg.Is<Purchase>(p =>
            p.SupplierId == cmd.SupplierId &&
            p.PurchaseDate == cmd.PurchaseDate &&
            p.Notes == cmd.Notes &&
            p.InvoiceNumber == cmd.InvoiceNumber &&
            p.TenantId == tenantId
        ));
        await _purchaseItemService.Received(1).CreatePurchaseItemsAsync(Arg.Any<Guid>(), cmd.PurchaseItems);
        await _purchaseInventoryService.Received(1).ProcessPurchaseInventoryAsync(Arg.Any<Guid>(), purchaseItems, cmd.PurchaseDate);
    }

    [Fact]
    public async Task deve_lancar_excecao_ao_tentar_registrar_compra_sem_tenantId()
    {
        //Arrange
        var cmd = _fixture.Create<CreatePurchaseCommand>();
        cmd.PurchaseItems = _fixture.CreateMany<CreatePurchaseItemCommand>().ToList();
        _tenantService.GetCurrentTenantId().Returns((Guid?)null);

        //Act
        Func<Task> act = async () => await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("User must belong to a tenant");
        await _purchaseRepository.DidNotReceive().AddAsync(Arg.Any<Purchase>());
    }

    [Fact]
    public async Task deve_lancar_excecao_ao_tentar_registrar_compra_sem_itens()
    {
        //Arrange
        var cmd = _fixture.Create<CreatePurchaseCommand>();
        cmd.PurchaseItems = new List<CreatePurchaseItemCommand>();
        var tenantId = _fixture.Create<Guid>();
        _tenantService.GetCurrentTenantId().Returns(tenantId);

        //Act
        Func<Task> act = async () => await _sut.Handle(cmd, CancellationToken.None);
        
        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Purchase must have at least one item");
        await _purchaseRepository.DidNotReceive().AddAsync(Arg.Any<Purchase>());
    }

    #endregion

    #region UpdatePurchase

    [Fact]
    public async Task deve_ser_possivel_atualizar_uma_compra()
    {
        //Arrange
        var cmd = _fixture.Create<UpdatePurchaseCommand>();
        cmd.PurchaseItems = _fixture.CreateMany<UpdatePurchaseItemCommand>().ToList();

        var existingPurchase = _fixture.Build<Purchase>()
            .With(p => p.Id, cmd.Id)
            .Create();

        var existingItems = _fixture.CreateMany<PurchaseItem>().ToList();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns(existingPurchase);
        _purchaseItemRepository.GetByPurchaseAsync(cmd.Id).Returns(existingItems);
        _purchaseItemService.UpdatePurchaseItemsAsync(cmd.Id, cmd.PurchaseItems, existingItems).Returns(_fixture.Create<decimal>());

        //Act
        await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await _purchaseInventoryService.Received(1).ProcessInventoryUpdateAsync(
            cmd.Id,
            existingItems,
            Arg.Any<List<PurchaseItem>>(),
            cmd.UserId
        );
        await _purchaseItemService.Received(1).UpdatePurchaseItemsAsync(cmd.Id, cmd.PurchaseItems, existingItems);
        await _purchaseRepository.Received(1).UpdateAsync(Arg.Is<Purchase>(p =>
            p.Id == cmd.Id &&
            p.SupplierId == cmd.SupplierId &&
            p.PurchaseDate == cmd.PurchaseDate &&
            p.Notes == cmd.Notes &&
            p.InvoiceNumber == cmd.InvoiceNumber
        ));
    }

    [Fact]
    public async Task nao_deve_ser_possivel_atualizar_compra_inexistente()
    {
        //Arrange
        var cmd = _fixture.Create<UpdatePurchaseCommand>();
        cmd.PurchaseItems = _fixture.CreateMany<UpdatePurchaseItemCommand>().ToList();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns((Purchase?)null);

        //Act
        Func<Task> act = async () => await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Purchase not found");
        await _purchaseRepository.DidNotReceive().UpdateAsync(Arg.Any<Purchase>());
    }

    [Fact]
    public async Task nao_deve_ser_possivel_atualizar_compra_sem_itens()
    {
        //Arrange
        var cmd = _fixture.Create<UpdatePurchaseCommand>();
        cmd.PurchaseItems = new List<UpdatePurchaseItemCommand>();

        var existingPurchase = _fixture.Build<Purchase>()
            .With(p => p.Id, cmd.Id)
            .Create();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns(existingPurchase);

        //Act
        Func<Task> act = async () => await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Purchase must have at least one item");
        await _purchaseRepository.DidNotReceive().UpdateAsync(Arg.Any<Purchase>());
    }

    #endregion

    #region DeletePurchase

    [Fact]
    public async Task deve_ser_possivel_deletar_uma_compra_com_itens()
    {
        //Arrange
        var cmd = _fixture.Create<DeletePurchaseCommand>();

        var existingPurchase = _fixture.Build<Purchase>()
            .With(p => p.Id, cmd.Id)
            .Create();

        var purchaseItems = _fixture.CreateMany<PurchaseItem>(3).ToList();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns(existingPurchase);
        _purchaseItemRepository.GetByPurchaseAsync(cmd.Id).Returns(purchaseItems);

        //Act
        await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await _purchaseInventoryService.Received(1).RevertPurchaseInventoryAsync(
            cmd.Id,
            purchaseItems,
            cmd.UserId
        );
        await _purchaseItemService.Received(1).DeletePurchaseItemsAsync(purchaseItems);
        await _purchaseRepository.Received(1).DeleteAsync(existingPurchase);
    }

    [Fact]
    public async Task deve_ser_possivel_deletar_uma_compra_sem_itens()
    {
        //Arrange
        var cmd = _fixture.Create<DeletePurchaseCommand>();

        var existingPurchase = _fixture.Build<Purchase>()
            .With(p => p.Id, cmd.Id)
            .Create();

        var emptyItems = new List<PurchaseItem>();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns(existingPurchase);
        _purchaseItemRepository.GetByPurchaseAsync(cmd.Id).Returns(emptyItems);

        //Act
        await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await _purchaseInventoryService.DidNotReceive().RevertPurchaseInventoryAsync(
            Arg.Any<Guid>(),
            Arg.Any<List<PurchaseItem>>(),
            Arg.Any<Guid>()
        );
        await _purchaseItemService.DidNotReceive().DeletePurchaseItemsAsync(Arg.Any<List<PurchaseItem>>());
        await _purchaseRepository.Received(1).DeleteAsync(existingPurchase);
    }

    [Fact]
    public async Task nao_deve_ser_possivel_deletar_compra_inexistente()
    {
        //Arrange
        var cmd = _fixture.Create<DeletePurchaseCommand>();

        _purchaseRepository.GetByIdAsync(cmd.Id).Returns((Purchase?)null);

        //Act
        Func<Task> act = async () => await _sut.Handle(cmd, CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Purchase not found");
        await _purchaseRepository.DidNotReceive().DeleteAsync(Arg.Any<Purchase>());
    }

    #endregion
}