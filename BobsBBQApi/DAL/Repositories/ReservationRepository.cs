using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;

namespace BobsBBQApi.DAL.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly BobsBBQContext _context;
    
    public ReservationRepository(BobsBBQContext context)
    {
        _context = context;
    }
    
    public List<DateTime> GetReservedSlots(DateTime date)
    {
        var targetDate = date.Date;

        // Get all reservations for the specified date
        var reservedHours = _context.Reservations
            .Where(r => r.ReservationDate == targetDate)
            .Select(r => r.TimeSlot) // TimeSlot is int
            .ToList();

        // Convert each hour to a DateTime on the target date
        var reservedSlots = reservedHours
            .Select(hour => targetDate.AddHours(hour))
            .ToList();

        return reservedSlots;
    }

    public void ReserveTable(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        _context.SaveChanges();
    }

    public bool IsTableReservedAt(Guid tableId, DateTime reservationDate, int timeSlotHour)
    {
        var targetDate = reservationDate.Date;
        var hour = timeSlotHour;

        return _context.Reservations.Any(r =>
            r.TableId == tableId &&
            r.ReservationDate.Date == targetDate &&
            r.TimeSlot == hour);
    }
}