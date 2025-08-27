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
    }
}