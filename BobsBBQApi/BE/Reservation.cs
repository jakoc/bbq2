using System.ComponentModel.DataAnnotations;


namespace BobsBBQApi.BE;

public class Reservation
{
    [Key]
    public Guid ReservationId { get; set; }
    public Guid TableId { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime TimeSlot { get; set; }
    public Guid UserId { get; set; }
    public int PartySize { get; set; }
    public string? Notes { get; set; }
}