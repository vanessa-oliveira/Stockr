using AutoFixture;
using FluentAssertions;
using Stockr.Domain.Common;
using Stockr.Domain.Entities;
using Stockr.Infrastructure.Repositories;
using Stockr.IntegrationTests.Configuration;

namespace Stockr.IntegrationTests.Repositories;

public class CategoryRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly CategoryRepository _repository;
    private readonly Fixture _fixture;

    public CategoryRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = new Fixture();
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _databaseFixture = fixture;
        _databaseFixture.ResetDatabase();
        _repository = new CategoryRepository(_databaseFixture.Context);
    }

    [Fact]
    public async Task deve_ser_possivel_adicionar_uma_nova_categoria()
    {
        // Arrange
        var category = _fixture.Create<Category>();

        // Act
        var result = await _repository.AddAsync(category);

        // Assert
        result.Should().BeTrue();
        var categoryResult = await _repository.GetByIdAsync(category.Id);
        categoryResult.Should().NotBeNull();
        categoryResult!.Name.Should().Be(category.Name);
        categoryResult.Description.Should().Be(category.Description);
        categoryResult.Active.Should().BeTrue();
        categoryResult.Deleted.Should().BeFalse();
    }

    [Fact]
    public async Task deve_ser_possivel_adicionar_uma_lista_de_categorias()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(3).ToList();

        // Act
        var result = await _repository.AddRangeAsync(categories);

        // Assert
        result.Should().BeTrue();
        var categoriesResult = await _repository.GetAllAsync();
        categoriesResult.Should().NotBeNullOrEmpty();
        categoriesResult.Should().HaveCount(categories.Count);
        categoriesResult.Should().Contain(c => c.Name == categories[0].Name);
    }

    [Fact]
    public async Task deve_retornar_a_categoria_a_partir_do_id_informado()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(3).ToList();
        await _repository.AddRangeAsync(categories);

        // Act
        var result = await _repository.GetByIdAsync(categories[1].Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(categories[1].Name);
        result.Description.Should().Be(categories[1].Description);
    }

    [Fact]
    public async Task deve_retornar_todas_as_categorias_registradas()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(3).ToList();
        await _repository.AddRangeAsync(categories);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(categories.Count);
        result.Should().OnlyContain(c => !c.Deleted);
    }

    [Fact]
    public async Task deve_retornar_as_categorias_de_forma_paginada()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(15).ToList();
        await _repository.AddRangeAsync(categories);

        var paginationParams = new PaginationParams
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(paginationParams);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task deve_retornar_segunda_pagina_de_categorias()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(15).ToList();
        await _repository.AddRangeAsync(categories);

        var paginationParams = new PaginationParams
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetPagedAsync(paginationParams);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task deve_atualizar_categoria_com_sucesso()
    {
        // Arrange
        var category = _fixture.Create<Category>();
        await _repository.AddAsync(category);

        var newName = _fixture.Create<string>();
        var newDescription = _fixture.Create<string>();

        // Act
        category.Name = newName;
        category.Description = newDescription;
        var result = await _repository.UpdateAsync(category);

        // Assert
        result.Should().BeTrue();
        var updatedCategory = await _repository.GetByIdAsync(category.Id);
        updatedCategory.Should().NotBeNull();
        updatedCategory!.Name.Should().Be(newName);
        updatedCategory.Description.Should().Be(newDescription);
    }

    [Fact]
    public async Task deve_marcar_como_deletado_ao_remover_categoria()
    {
        // Arrange
        var category = _fixture.Create<Category>();
        await _repository.AddAsync(category);

        // Act
        var result = await _repository.DeleteAsync(category);

        // Assert
        result.Should().BeTrue();
        var categoryResult = await _repository.GetDeletedByIdAsync(category.Id);
        categoryResult.Should().NotBeNull();
        categoryResult.Deleted.Should().BeTrue();
    }

    [Fact]
    public async Task deve_excluir_categoria_deletada_dos_resultados_paginados()
    {
        // Arrange
        var category = _fixture.Create<Category>();
        await _repository.AddAsync(category);

        // Act
        await _repository.DeleteAsync(category);

        var paginationParams = new PaginationParams
        {
            PageNumber = 1,
            PageSize = 10
        };

        var pagedResult = await _repository.GetPagedAsync(paginationParams);

        // Assert
        pagedResult.Items.Should().BeEmpty();
        pagedResult.TotalCount.Should().Be(0);
    }
}