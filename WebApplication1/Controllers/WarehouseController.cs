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

    [HttpPost("AddProduct")]
    public async Task<IActionResult> AddProduct(Warehouse warehouse)
    {

        if (warehouse.Amount <= 0 )
        {
            return BadRequest("Amount should be higher than 0");
        }

        string result = await _warehouseService.AddProduct(warehouse);

        return Ok(result);
    }

    [HttpPost("AddByProc")]
    public async Task<IActionResult> AddByProc(Warehouse warehouse)
    {
        string result = await _warehouseService.AddNewProductByProcedure(warehouse);

        return Ok(result);
    }


}
