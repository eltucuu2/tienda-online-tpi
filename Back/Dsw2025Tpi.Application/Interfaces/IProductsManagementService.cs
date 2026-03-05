using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IProductsManagementService
    {
        Task<ProductModel.Response> AddProduct(ProductModel.Request request);
        Task<ProductModel.ResponsePagination?> GetProducts(ProductModel.FilterProduct filter);
        Task<Product?> GetProductById(Guid id);
        Task<ProductModel.Response> Update(Guid id, ProductModel.Request request);
        Task<bool> DisableProduct(Guid id);
    }
}