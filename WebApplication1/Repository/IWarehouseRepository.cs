namespace WebApplication1.Repository;

public interface IWarehouseRepository
{
    Task<bool> CheckIfCompletedOrdersExist(Warehouse warehouse);
    Task<bool> VerifyExistingOrder(Warehouse warehouse);
    Task<bool> VerifyExistingProduct(Warehouse warehouse);
    Task<bool> VerifyExistingWarehouse(Warehouse warehouse);
    Task<int> InsertNewOrder(Warehouse warehouse);
    Task<string> ExecuteStoredProcedure(Warehouse warehouse);
}
