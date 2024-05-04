namespace WebApplication1.Service

{
    public interface IWarehouseService
    {
        Task<string> AddNewProductQuery(Warehouse warehouse);

        Task<string> AddNewProductByProcedure(Warehouse warehouse);
    }
}