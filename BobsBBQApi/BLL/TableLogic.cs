using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;

namespace BobsBBQApi.BLL.Interfaces;

public class TableLogic : ITableLogic
{
    private readonly ITableRepository _tableRepository;
    
    public TableLogic(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }
    public void AddTable( int capacity, int tableNumber)
    {
        if (capacity <= 0 || tableNumber <= 0)
        {
            throw new ArgumentException("Capacity and table number must be greater than zero.");
        }
        var table = new Table
        {
            TableId = Guid.NewGuid(),
            Capacity = capacity,
            TableNumber = tableNumber
        };
        
        _tableRepository.AddTable(table);
    }
    public IEnumerable<Table> GetTables()
    {
        return _tableRepository.GetTables();
    }
}