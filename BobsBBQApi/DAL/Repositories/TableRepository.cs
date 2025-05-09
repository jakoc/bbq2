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
            MonitorService.Log.Information("Adding table {@TableId} with capacity {@Capacity}", table.TableId, table.Capacity);

            _context.RestaurantTables.Add(table);
            _context.SaveChanges();

            MonitorService.Log.Information("Table {@TableId} successfully added", table.TableId);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error occurred while adding table {@TableId}", table.TableId);
            throw;
        }
    }
    
    public IEnumerable<Table> GetTables()
    {
        MonitorService.Log.Information("Fetching all tables from the database");

        var tables = _context.RestaurantTables.ToList();

        MonitorService.Log.Information("Fetched {@TableCount} tables", tables.Count);

        return tables;
    }
    
}