using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;
using BobsBBQApi.Services;

namespace BobsBBQApi.BLL;

public class ReservationLogic : IReservationLogic
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IUserRepository _userRepository;
    public ReservationLogic(IReservationRepository reservationRepository, ITableRepository tableRepository, IUserRepository userRepository)
    {
        _tableRepository = tableRepository;
        _userRepository = userRepository;
        _reservationRepository = reservationRepository;
    }
    public List<DateTime> GetAvailableTimeSlot(DateTime date, int partySize)
    {
        MonitorService.Log.Information("GetAvailableTimeSlot called with date: {@Date} and party size: {@PartySize}", date, partySize);
        var tables = _tableRepository.GetTables()
            .Where(t => t.Capacity >= partySize)
            .ToList();

        if (!tables.Any())
        {
            MonitorService.Log.Warning("No tables available for party size: {@PartySize}", partySize);
            throw new ArgumentException("No tables available for this party size.");
        }
        
        
        var availableSlots = new List<DateTime>();
        try
        {
            MonitorService.Log.Information("Checking available time slots for date: {@Date}", date);
            var defaultSlots = DefaultTimeSlots.GetSlots(date);
            foreach (var slot in defaultSlots)
            {
                int slotHour = slot.Hour;

                bool hasFreeTable = tables.Any(table => 
                    !_reservationRepository.IsTableReservedAt(table.TableId, date, slotHour));

                if (hasFreeTable)
                {
                    availableSlots.Add(slot);
                }
            }
        }
        catch (Exception e)
        {
            MonitorService.Log.Error(e, "Error while checking available time slots for date: {@Date}", date);
            throw new ArgumentException("Error while checking available time slots", e);
        }
        

        return availableSlots;
    }
    
    

        
    public void ReserveTable(DateTime reservationDate, int timeSlot, int partySize, string note, Guid userId)
    {
        MonitorService.Log.Information("ReserveTable called with date: {@ReservationDate}, time slot: {@TimeSlot}, party size: {@PartySize}, note: {@Note}, userId: {@UserId}", 
            reservationDate, timeSlot, partySize, note, userId);
        ValidateReservationInputs(reservationDate, timeSlot, partySize, userId);
        MonitorService.Log.Information("Inputs validated successfully");
        try
        {
            var tables = _tableRepository.GetTables()
                .Where(t => t.Capacity >= partySize)
                .ToList();

            MonitorService.Log.Information("Found {@TableCount} suitable tables for party size {@PartySize}", tables.Count, partySize);

            var availableTables = tables
                .Where(t => !_reservationRepository.IsTableReservedAt(t.TableId, reservationDate, timeSlot))
                .ToList();

            if (!availableTables.Any())
            {
                MonitorService.Log.Warning("No available tables for reservation date: {@ReservationDate}, time slot: {@TimeSlot}", reservationDate, timeSlot);
                throw new InvalidOperationException("No available table at this time slot.");
            }

            var selectedTable = availableTables[new Random().Next(availableTables.Count)];


            var reservation = new Reservation
            {
                ReservationId = Guid.NewGuid(),
                TableId = selectedTable.TableId,
                ReservationDate = reservationDate,
                UserId = userId,
                PartySize = partySize,
                TimeSlot = timeSlot,
                Note = note
            };

            _reservationRepository.ReserveTable(reservation);
            MonitorService.Log.Information("Reservation successfully saved with ID {@ReservationId}", reservation.ReservationId);
        }

        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Unexpected error occurred while reserving a table");
            throw;
        }
    }

    private  void ValidateReservationInputs(DateTime reservationDate, int timeSlot, int partySize,
        Guid userId)
    {
        MonitorService.Log.Information("Validating reservation inputs: {@ReservationDate}, {@TimeSlot}, {@PartySize}, {@UserId}", 
            reservationDate, timeSlot, partySize, userId);
        if (reservationDate.Date < DateTime.Now.Date)
            throw new ArgumentException("Reservation date cannot be in the past.");
        

        if (reservationDate.Date < DateTime.Now.Date)
        {
            MonitorService.Log.Warning("Validation failed: reservation date is in the past ({@ReservationDate})", reservationDate);
            throw new ArgumentException("Reservation date cannot be in the past.");
        }

        if (timeSlot <= 9 || timeSlot >= 23)
        {
            MonitorService.Log.Warning("Validation failed: invalid time slot ({@TimeSlot})", timeSlot);
            throw new ArgumentException("Time slot must be between 10 AM and 10 PM.");
        }

        if (partySize <= 0 || partySize >= 11)
        {
            MonitorService.Log.Warning("Validation failed: invalid party size ({@PartySize})", partySize);
            throw new ArgumentException("Party size must be between 1 and 10.");
        }

        if (userId == Guid.Empty)
        {
            MonitorService.Log.Warning("Validation failed: user ID is empty");
            throw new ArgumentException("User ID cannot be empty.");
        }

        var availableTimeSlots = GetAvailableTimeSlot(reservationDate, partySize);
        if (!availableTimeSlots.Any(x => x.Hour == timeSlot))
        {
            MonitorService.Log.Warning("Validation failed: selected time slot {@TimeSlot} not available for date {@ReservationDate}", timeSlot, reservationDate);
            throw new ArgumentException("The selected time slot is not available.");
        }
    }
}