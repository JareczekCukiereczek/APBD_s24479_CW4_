using System;
using System.Data;
using Microsoft.Data.SqlClient;
using WebApplication1.Controllers;

namespace WebApplication1.Repository
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> CheckIfCompletedOrdersExist(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "IF EXISTS (SELECT * FROM [master].[dbo].[Product_Warehouse] WHERE IdOrder = (SELECT IdOrder FROM [master].[dbo].[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount)) " +
                "BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END", connection);
            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 2;
        }

        public async Task<bool> CheckOrder(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                @"SELECT 
                          CASE 
                          WHEN EXISTS (
                          SELECT * FROM [master].[dbo].[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < GETDATE()
                          ) THEN 1 
                          ELSE 2 
                          END", connection);

            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 1;
        }

        public async Task<bool> CheckProductExists(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "IF EXISTS (SELECT * FROM [master].[dbo].[Product] WHERE IdProduct = @IdProduct) " +
                "BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END", connection);
            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 1;
        }

        

        private async Task UpdateFull(DateTime createdAt, decimal orderId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "UPDATE [master].[dbo].[Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @OrderId", connection);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@OrderId", orderId);

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Update executed");
        }
        public async Task<bool> CheckWareHouseExists(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                @"SELECT 
                        CASE 
                        WHEN EXISTS (
                        SELECT * FROM [master].[dbo].[Warehouse] 
                        WHERE IdWarehouse = @IdWarehouse
                        )THEN 1 
                        ELSE 2 
                        END", connection);

            command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 1;
        }

        public async Task<int> InsertOrder(Warehouse warehouse) {
        int orderId;
        await using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"])){
            await connection.OpenAsync();

            using (var command = new SqlCommand(
            "INSERT INTO [master].[dbo].[Order] ([IdProduct], [Amount], [CreatedAt], [FulfilledAt]) " +
            "VALUES (@IdProduct, @Amount, @CreatedAt, null); SELECT SCOPE_IDENTITY()", connection))
            {
                command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
                command.Parameters.AddWithValue("@Amount", warehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedDateTime);

                var orderIdentity = await command.ExecuteScalarAsync();
                orderId = Convert.ToInt32(orderIdentity);
                await UpdateFull(warehouse.CreatedDateTime, (decimal)orderIdentity);
            }
        }
        return orderId;
    }

    public async Task<int> InsertProductToWarehouse(Warehouse warehouse, int orderId) {
        int productWarehouseId;
        await using (var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]))
        {
            await connection.OpenAsync();

            using var command = new SqlCommand(@"
            DECLARE @ProductId INT;
            SELECT @ProductId = [Price] FROM [master].[dbo].[Product] WHERE [IdProduct] = @IdProduct;
            INSERT INTO [master].[dbo].[Product_Warehouse] ([IdWarehouse], [IdProduct], [IdOrder], [Amount], [Price], [CreatedAt]) 
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @ProductId, @CreatedAt);
            SELECT SCOPE_IDENTITY()", connection);
            {
                command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
                command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);
                command.Parameters.AddWithValue("@IdOrder", orderId);
                command.Parameters.AddWithValue("@Amount", warehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedDateTime);

                var productPrice = await command.ExecuteScalarAsync();

                command.Parameters.AddWithValue("@Price", warehouse.Amount * (int)productPrice);

                var idProductWarehouse = await command.ExecuteScalarAsync();
                productWarehouseId = Convert.ToInt32(idProductWarehouse);
            }
        }
        return productWarehouseId;
    }


public async Task<string> AddNewProductByProcedure(Warehouse warehouse)
{
    try
    {
        string result = await ExecProc(warehouse);
        return result;
    }
    catch (SqlException ex)
    {
        return ex.Message; 
    }
}

public async Task<string> ExecProc(Warehouse warehouse)
{
    try
    {
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using var command = new SqlCommand("AddProductToWarehouse", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        
        if (ProductExists(warehouse.ProductId))
        {
            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
            command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedDateTime);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return result.ToString();
        }
        else
        {
            return "Product doesn't exists";
        }
    }
    catch (SqlException ex)
    {
        return ex.Message; 
    }
}
private bool ProductExists(int productId)
{
    try
    {
        using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        using var command = new SqlCommand("SELECT COUNT(*) FROM Products WHERE Id = @IdProduct", connection);

        command.Parameters.AddWithValue("@IdProduct", productId);

        connection.Open();
        int count = (int)command.ExecuteScalar();
        return count > 0;
    }
    catch (SqlException){
          return false; 
    }
    }
    }
}
