
using WebApplication1.Repository;
using System.Threading.Tasks;
using WebApplication1.Service;

namespace WebApplication1.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _repository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _repository = warehouseRepository;
        }

        public async Task<string> AddProduct(Warehouse warehouse)
        {
            bool productAndWareHouseExists = await CheckProductAndWareHouseExists(warehouse);

            if (!productAndWareHouseExists)
            {
                return "Warehouse or product doesn't exists";
            }

            bool isOrderExisting = await CheckOrder(warehouse);
            bool isCompletedOrdersExist = await _repository.CheckIfCompletedOrdersExist(warehouse);

            if (!isOrderExisting && isCompletedOrdersExist)
            {
                int result = await _repository.InsertOrder(warehouse);
                return "Key value for Product_Warehouse: " + result.ToString();
            }
            else
            {
                return "Product order exists";
            }
        }

        public async Task<bool> CheckProductAndWareHouseExists(Warehouse warehouse)
        {
            bool productExists = await _repository.CheckProductExists(warehouse);
            bool wareHouseExists = await _repository.CheckWareHouseExists(warehouse);
    
            return productExists && wareHouseExists;
        }

        public async Task<bool> CheckOrder(Warehouse warehouse)
        {
            return await _repository.CheckOrder(warehouse);
        }

        public async Task<string> AddNewProductByProcedure(Warehouse warehouse)
        {
            string result = await _repository.ExecProc(warehouse);
            return result;
        }
    }
}