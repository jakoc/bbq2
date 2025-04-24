using System.ComponentModel.DataAnnotations;

namespace BobsBBQApi.BE;

public class Table
{
    [Key]
    public Guid TableId { get; set; }
    public int TableNumber { get; set; }
    public int Capacity { get; set; }
}