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
            .Ignore(dest => dest.Email)
            .Ignore(dest => dest.Address);

        config.NewConfig<SupplierViewModel, Supplier>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Phone, src => src.Phone);

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
    }
}