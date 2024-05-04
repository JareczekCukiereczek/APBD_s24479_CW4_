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

        public async Task<bool> VerifyExistingOrder(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "IF EXISTS (SELECT * FROM [master].[dbo].[Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < GETDATE()) " +
                "BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END", connection);
            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 1;
        }

        public async Task<bool> VerifyExistingProduct(Warehouse warehouse)
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

        public async Task<bool> VerifyExistingWarehouse(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "IF EXISTS (SELECT * FROM [master].[dbo].[Warehouse] WHERE IdWarehouse = @IdWarehouse) " +
                "BEGIN SELECT 1 END ELSE BEGIN SELECT 2 END", connection);
            command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);

            var queryResult = await command.ExecuteScalarAsync();
            return Convert.ToInt32(queryResult) == 1;
        }

        private async Task UpdateFulfilledAtAsync(DateTime createdAt, decimal orderId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "UPDATE [master].[dbo].[Order] SET FulfilledAt = @CreatedAt WHERE IdOrder = @OrderId", connection);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@OrderId", orderId);

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("UpdateFulfilledAt executed");
        }

        public async Task<int> InsertNewOrder(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "INSERT INTO [master].[dbo].[Order] ([IdProduct], [Amount], [CreatedAt], [FulfilledAt]) " +
                "VALUES (@IdProduct, @Amount, @CreatedAt, null); SELECT SCOPE_IDENTITY()", connection);
            command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedDateTime);

            var orderIdentity = await command.ExecuteScalarAsync();
            await UpdateFulfilledAtAsync(warehouse.CreatedDateTime, (decimal)orderIdentity);

            command.CommandText = "SELECT [Price] FROM [master].[dbo].[Product] WHERE [IdProduct] = @IdProduct";
            var productPrice = await command.ExecuteScalarAsync();

            command.CommandText = "INSERT INTO [master].[dbo].[Product_Warehouse] ([IdWarehouse], [IdProduct], [IdOrder], [Amount], [Price], [CreatedAt]) " +
                                         "VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt); SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);
            command.Parameters.AddWithValue("@IdOrder", orderIdentity);
            command.Parameters.AddWithValue("@Price", warehouse.Amount * (decimal)productPrice);

            var idProductWarehouse = await command.ExecuteScalarAsync();
            return (int)idProductWarehouse;
        }

        public async Task<string> ExecuteStoredProcedure(Warehouse warehouse)
        {
            try
            {
                await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
                await using var command = new SqlCommand("AddProductToWarehouse", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@IdProduct", warehouse.ProductId);
                command.Parameters.AddWithValue("@IdWarehouse", warehouse.WarehouseId);
                command.Parameters.AddWithValue("@Amount", warehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedDateTime);

                await connection.OpenAsync();

                var result = await command.ExecuteScalarAsync();
                return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
