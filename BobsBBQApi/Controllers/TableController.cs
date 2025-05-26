using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

[Route("api/[controller]")] 
public class TableController : ControllerBase
{
    private readonly ITableLogic _tableLogic;

    public TableController(ITableLogic tableLogic)
    {
        _tableLogic = tableLogic;
    }

    [HttpPost("AddTable")]
    public IActionResult AddTable([FromBody] AddTableDto dto)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("Add table called from controller");
        if (dto == null)
        {
            MonitorService.Log.Warning("Invalid table data received");
            return BadRequest("Invalid table data.");
        }
        if (!ModelState.IsValid)
        {
            MonitorService.Log.Warning("Model state is invalid");
            return BadRequest(ModelState);
        }
        int capacity = dto.Capacity;
        activity?.SetTag("table.capacity", capacity);
        if (capacity <= 0)
        {
            MonitorService.Log.Warning("Invalid table capacity received: {@Capacity}", capacity);
            return BadRequest("Capacity must be greater than zero.");
        }

        try
        {
            MonitorService.Log.Information("AddTable in TableLogic called from controller");
            _tableLogic.AddTable(capacity);
            MonitorService.Log.Information("Table added successfully with capacity: {@Capacity}", capacity);
            return Ok("Table added successfully.");
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error adding table with capacity: {@Capacity}", capacity);
            return StatusCode(500, "Error adding table: " + ex.Message);
        }
    }
}