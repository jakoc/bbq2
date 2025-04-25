using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;

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
        _context.RestaurantTables.Add(table);
        _context.SaveChanges();
    }
    
    public IEnumerable<Table> GetTables()
    {
        return _context.RestaurantTables.ToList();
    }
    
}