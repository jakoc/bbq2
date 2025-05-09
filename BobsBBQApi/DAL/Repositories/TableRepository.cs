using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Services;

namespace BobsBBQApi.DAL.Repositories;

public class TableRepository : ITableRepository
{
    private readonly BobsBBQContext _context;
    
    public TableRepository(BobsBBQContext context)
    {
        _context = context;
    }
    
    public void AddTable(Table table)
    {
        try
        {
            MonitorService.Log.Information("Adding table {@tableId} with capacity {@capacity}", table.TableId, table.Capacity);

            _context.RestaurantTables.Add(table);
            _context.SaveChanges();

            MonitorService.Log.Information("Table {@tableId} successfully added", table.TableId);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error occurred while adding table {@tableId}", table.TableId);
            throw;
        }
    }
    
    public IEnumerable<Table> GetTables()
    {
        MonitorService.Log.Information("Fetching all tables from the database");

        var tables = _context.RestaurantTables.ToList();

        MonitorService.Log.Information("Fetched {@tableCount} tables", tables.Count);

        return tables;
    }
    
}