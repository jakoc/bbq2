using BobsBBQApi.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class TableController : ControllerBase
{
    private readonly ITableLogic _tableLogic;

    public TableController(ITableLogic tableLogic)
    {
        _tableLogic = tableLogic;
    }

    [HttpPost("[action]")]
    public IActionResult AddTable(int capacity, int tableNumber)
    {
        if (capacity <= 0 || tableNumber <= 0)
        {
            return BadRequest("Capacity and table number must be greater than zero.");
        }

        try
        {
            _tableLogic.AddTable(capacity);
            return Ok("Table added successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error adding table: " + ex.Message);
        }
    }
}