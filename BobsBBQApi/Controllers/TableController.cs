using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
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
    public IActionResult AddTable(int capacity)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("Add table called from controller");
        activity?.SetTag("table.capacity", capacity);
        if (capacity <= 0)
        {
            MonitorService.Log.Warning("Invalid table capacity received: {@capacity}", capacity);
            return BadRequest("Capacity must be greater than zero.");
        }

        try
        {
            MonitorService.Log.Information("AddTable in TableLogic called from controller");
            _tableLogic.AddTable(capacity);
            MonitorService.Log.Information("Table added successfully with capacity: {@capacity}", capacity);
            return Ok("Table added successfully.");
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error adding table with capacity: {@capacity}", capacity);
            return StatusCode(500, "Error adding table: " + ex.Message);
        }
    }
}