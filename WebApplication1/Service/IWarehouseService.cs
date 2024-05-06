namespace WebApplication1.Service

{
    public interface IWarehouseService
    {
        Task<string> AddProduct(Warehouse warehouse);
        Task<bool> CheckProductAndWareHouseExists(Warehouse warehouse);
        Task<bool> CheckOrder(Warehouse warehouse);
        Task<string> AddNewProductByProcedure(Warehouse warehouse);
    }
}