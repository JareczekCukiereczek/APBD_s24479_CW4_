
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

        public async Task<string> AddNewProductQuery(Warehouse warehouse)
        {
            bool isProductExisting = await _repository.VerifyExistingProduct(warehouse);
            bool isWarehouseExisting = await _repository.VerifyExistingWarehouse(warehouse);

            if (isProductExisting && isWarehouseExisting)
            {
                bool isOrderExisting = await _repository.VerifyExistingOrder(warehouse);
                bool isCompletedOrdersExist = await _repository.CheckIfCompletedOrdersExist(warehouse);

                if (!isOrderExisting && isCompletedOrdersExist)
                {
                    int result = await _repository.InsertNewOrder(warehouse);
                    return "Wartość klucza z Product_Warehouse: " + result.ToString();
                }
                else
                {
                    return "Taki order już istnieje";
                }
            }
            else
            {
                return "Magazyn lub produkt nie istnieje";
            }
        }

        public async Task<string> AddNewProductByProcedure(Warehouse warehouse)
        {
            string result = await _repository.ExecuteStoredProcedure(warehouse);
            return result;
        }
    }
}