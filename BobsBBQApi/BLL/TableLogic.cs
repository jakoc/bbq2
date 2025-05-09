using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Services;

namespace BobsBBQApi.BLL.Interfaces;

public class TableLogic : ITableLogic
{
    private readonly ITableRepository _tableRepository;
    
    public TableLogic(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }
    public void AddTable( int capacity)
    {
        MonitorService.Log.Information("AddTable called in logic with capacity: {@Capacity}", capacity);
        if (capacity <= 0 )
        {
            MonitorService.Log.Warning("Invalid table capacity received: {@Capacity}", capacity);
            throw new ArgumentException("Capacity and table number must be greater than zero.");
        }

        try
        {
            var tableNumber = _tableRepository.GetTables().Count();
            var table = new Table
            {
                TableId = Guid.NewGuid(),
                Capacity = capacity,
                TableNumber = tableNumber +1
            };
        
            _tableRepository.AddTable(table);
        }
        catch (Exception e)
        {
            MonitorService.Log.Error(e, "Error while adding table with capacity: {@Capacity}", capacity);
            throw new ArgumentException( "Error while adding table", e);
        }
        
    }
    public IEnumerable<Table> GetTables()
    {
        MonitorService.Log.Information("GetTables called in logic");
        return _tableRepository.GetTables();
    }
}