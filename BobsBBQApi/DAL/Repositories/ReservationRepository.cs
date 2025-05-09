using BobsBBQApi.BE;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.Services;

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
        MonitorService.Log.Information("GetReservedSlots called with date: {@date}", date);
        var targetDate = date.Date;

        // Get all reservations for the specified date
        var reservedHours = _context.Reservations
            .Where(r => r.ReservationDate == targetDate)
            .Select(r => r.TimeSlot) // TimeSlot is int
            .ToList();
        MonitorService.Log.Information("Found {@ReservedSlotCount} reserved slots for date {@TargetDate}",
            reservedHours.Count, targetDate);

        // Convert each hour to a DateTime on the target date
        var reservedSlots = reservedHours
            .Select(hour => targetDate.AddHours(hour))
            .ToList();

        return reservedSlots;
    }

    public void ReserveTable(Reservation reservation)
    {
        try
        {
            MonitorService.Log.Information("ReserveTable called for reservation {@ReservationId} with table ID {@TableId}, time slot {@TimeSlot}, party size {@PartySize}, user ID {@UserId}",
                reservation.ReservationId, reservation.TableId, reservation.TimeSlot, reservation.PartySize, reservation.UserId);
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            MonitorService.Log.Information("Reservation {@ReservationId} successfully saved", reservation.ReservationId);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error while saving reservation {@ReservationId}", reservation.ReservationId);
            throw;
        }
    }

    public bool IsTableReservedAt(Guid tableId, DateTime reservationDate, int timeSlotHour)
    {
        MonitorService.Log.Information("Checking if table {@TableId} is reserved at {@ReservationDate} for time slot {@TimeSlotHour}", tableId, reservationDate, timeSlotHour);

        var targetDate = reservationDate.Date;
        var hour = timeSlotHour;

        var isReserved = _context.Reservations.Any(r =>
            r.TableId == tableId &&
            r.ReservationDate.Date == targetDate &&
            r.TimeSlot == hour);

        MonitorService.Log.Information("Table {@TableId} is {@ReservationStatus} at {@ReservationDate} for time slot {@TimeSlotHour}",
            tableId, isReserved ? "reserved" : "available", reservationDate, timeSlotHour);

        return isReserved;
    }
}