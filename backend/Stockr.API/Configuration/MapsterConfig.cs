using Mapster;
using Stockr.Application.Models;
using Stockr.Domain.Entities;

namespace Stockr.API.Configuration;

public static class MapsterConfig
{
    public static void ConfigureMapster(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);
        
        config.NewConfig<CategoryViewModel, Category>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description);

        config.NewConfig<Product, ProductViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.SKU, src => src.SKU)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Category, src => src.Category)
            .Map(dest => dest.Supplier, src => src.Supplier)
            .Map(dest => dest.CostPrice, src => src.CostPrice)
            .Map(dest => dest.SalePrice, src => src.SalePrice)
            .Map(dest => dest.MinStock, src => src.MinStock);

        config.NewConfig<ProductViewModel, Product>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.SKU, src => src.SKU)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CostPrice, src => src.CostPrice)
            .Map(dest => dest.SalePrice, src => src.SalePrice)
            .Map(dest => dest.MinStock, src => src.MinStock);

        config.NewConfig<Supplier, SupplierViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Active, src => src.Active)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        config.NewConfig<SupplierViewModel, Supplier>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Active, src => src.Active);

        config.NewConfig<Customer, CustomerViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.CPF, src => src.CPF)
            .Map(dest => dest.CNPJ, src => src.CNPJ);

        config.NewConfig<CustomerViewModel, Customer>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.CPF, src => src.CPF)
            .Map(dest => dest.CNPJ, src => src.CNPJ);

        config.NewConfig<Sale, SaleViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.Customer, src => src.Customer)
            .Map(dest => dest.SalespersonId, src => src.SalesPersonId)
            .Map(dest => dest.SalespersonName, src => src.Salesperson.Name)
            .Map(dest => dest.SaleStatus, src => src.SaleStatus)
            .Map(dest => dest.TotalAmount, src => src.TotalAmount)
            .Map(dest => dest.SaleDate, src => src.SaleDate)
            .Map(dest => dest.SaleItems, src => src.SaleItems);

        config.NewConfig<SaleItem, SaleItemViewModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.TotalPrice, src => src.TotalPrice);

        config.NewConfig<SaleViewModel, Sale>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.SalesPersonId, src => src.SalespersonId)
            .Map(dest => dest.SaleStatus, src => src.SaleStatus)
            .Map(dest => dest.TotalAmount, src => src.TotalAmount)
            .Map(dest => dest.SaleDate, src => src.SaleDate)
            .Ignore(dest => dest.Customer)
            .Ignore(dest => dest.Salesperson)
            .Ignore(dest => dest.SaleItems);
    }
}