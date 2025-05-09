namespace BobsBBQApi.BE;

public class ReserveTableDto
{
    public DateTime ReservationDate { get; set; }
    public int TimeSlot { get; set; }
    public int PartySize { get; set; }
    public string? Note { get; set; }
    public Guid UserId { get; set; }
}
