using BobsBBQApi.BE;

namespace BobsBBQApi.DAL.Repositories.Interfaces;

public interface ITableRepository
{
    void AddTable(Table table);
    IEnumerable<Table> GetTables();
}