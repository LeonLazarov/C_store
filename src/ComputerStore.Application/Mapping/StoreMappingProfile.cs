using AutoMapper;
using ComputerStore.Application.Dtos;
using ComputerStore.Domain.Entities;

namespace ComputerStore.Application.Mapping;

public sealed class StoreMappingProfile : Profile
{
    public StoreMappingProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.Categories), opt => opt.MapFrom(product =>
                product.ProductCategories.Select(productCategory => productCategory.Category.Name).ToArray()));
    }
}
