using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;

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
        var tables = _tableRepository.GetTables()
            .Where(t => t.Capacity >= partySize)
            .ToList();

        if (!tables.Any())
            throw new ArgumentException("No tables available for this party size.");

        var defaultSlots = DefaultTimeSlots.GetSlots(date);
        var availableSlots = new List<DateTime>();

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

        return availableSlots;
    }
    
    

        
    public void ReserveTable(DateTime reservationDate, int timeSlot, int partySize, string note, Guid userId)
    {
        ValidateReservationInputs(reservationDate, timeSlot, partySize, userId);
    
        var tables = _tableRepository.GetTables()
            .Where(t => t.Capacity >= partySize)
            .ToList();
    
        var availableTables = tables
            .Where(t => !_reservationRepository.IsTableReservedAt(t.TableId, reservationDate ,timeSlot))
            .ToList();
    
        if (!availableTables.Any())
            throw new InvalidOperationException("No available table at this time slot.");
    
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
    }

    private  void ValidateReservationInputs(DateTime reservationDate, int timeSlot, int partySize,
        Guid userId)
    {
 
        if (reservationDate.Date < DateTime.Now.Date)
            throw new ArgumentException("Reservation date cannot be in the past.");

        if (timeSlot <= 9 || timeSlot >= 23)
            throw new ArgumentException("Time slot must be between 10 AM and 10 PM.");

        if (partySize <= 0 || partySize >= 11)
            throw new ArgumentException("Party size must be between 1 and 10.");

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.");
        
        var availableTimeSlots = GetAvailableTimeSlot(reservationDate, partySize);

        if (!availableTimeSlots.Any(x => x.Hour == timeSlot))
            throw new ArgumentException("The selected time slot is not available.");
    }
}