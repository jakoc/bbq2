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
    public void AddTable( int capacity)
    {
        if (capacity <= 0 )
        {
            throw new ArgumentException("Capacity and table number must be greater than zero.");
        }

        var tableNumber = _tableRepository.GetTables().Count();
        var table = new Table
        {
            TableId = Guid.NewGuid(),
            Capacity = capacity,
            TableNumber = tableNumber +1
        };
        
        _tableRepository.AddTable(table);
    }
    public IEnumerable<Table> GetTables()
    {
        return _tableRepository.GetTables();
    }
}