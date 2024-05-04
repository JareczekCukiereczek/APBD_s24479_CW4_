using WebApplication1.Service;

namespace WebApplication1.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;


[Route("api/warehouse")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost("AddNewProduct")]
    public async Task<IActionResult> AddNewProduct(Warehouse warehouse)
    {

        //walidacja czy ilość przekazana w żądaniu jest większa od 0

        if (warehouse.Amount <= 0 )
        {
            return BadRequest("Amount musi być większe od zera");
        }

        string result = await _warehouseService.AddNewProductQuery(warehouse);

        return Ok(result);
    }

    [HttpPost("AddNewProductByProcedure")]
    public async Task<IActionResult> AddNewProductByProcedure(Warehouse warehouse)
    {
        string result = await _warehouseService.AddNewProductByProcedure(warehouse);

        return Ok(result);
    }


}
