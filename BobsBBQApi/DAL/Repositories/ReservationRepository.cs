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
        var reservedSlots = _context.Reservations
            .Where(r => r.ReservationDate.Date == date.Date)
            .Select(r => r.TimeSlot)
            .ToList();
    

        return reservedSlots;
    }

    public void ReserveTable(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        _context.SaveChanges();
    }
}