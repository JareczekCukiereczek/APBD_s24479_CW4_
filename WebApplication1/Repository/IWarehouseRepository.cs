namespace WebApplication1.Repository;

public interface IWarehouseRepository
{
    Task<bool> CheckIfCompletedOrdersExist(Warehouse warehouse);
    Task<bool> CheckOrder(Warehouse warehouse);
    Task<bool> CheckWareHouseExists(Warehouse warehouse);
    Task<bool> CheckProductExists(Warehouse warehouse);
    Task<int> InsertOrder(Warehouse warehouse);
    Task<int> InsertProductToWarehouse(Warehouse warehouse,int orderId);
    Task<string> ExecProc(Warehouse warehouse);
}
