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
        var reservedSlots = _reservationRepository.GetReservedSlots(date);
        var allTables = _tableRepository.GetTables();
        var defaultSlots = DefaultTimeSlots.GetSlots(date);
        var availableTimeSlots = new List<DateTime>();

        foreach(var table in allTables)
        {
            if (table.Capacity < partySize)
            {
                throw new ArgumentException("Party size exceeds table capacity.");
            }
            var availableslots = defaultSlots
                .Where(slot => !reservedSlots.Contains(slot))
                .ToList();
            if (availableslots.Any())
            {
                availableTimeSlots.AddRange(availableslots);
            }

        }

        return availableTimeSlots;
    }
    
    
    public void ReserveTable( DateTime reservationDate, DateTime timeSlot, int partySize, string note, Guid userId, int tableNumber)
    {
        
        
        ValidateReservationInputs(reservationDate, timeSlot, partySize, userId, tableNumber);
  
        var table = _tableRepository.GetTables().FirstOrDefault(x=> x.TableNumber == tableNumber);
        var tableId = table.TableId;
        
        if (table == null)
        {
            throw new ArgumentException("Table not found.");
        }
        
        var reservation = new Reservation
        {
            ReservationId = Guid.NewGuid(),
            TableId = tableId,
            UserId = userId,
            ReservationDate = reservationDate,
            TimeSlot = timeSlot,
            PartySize = partySize,
            Note = note
        };
        _reservationRepository.ReserveTable(reservation);
    }

    private  void ValidateReservationInputs(DateTime reservationDate, DateTime timeSlot, int partySize,
        Guid userId, int tableNumber)
    {
        
        var availableTimeSlots = GetAvailableTimeSlot(reservationDate, partySize);
        
        if (!availableTimeSlots.Any(x => x.Hour == timeSlot.Hour))
        {
            throw new ArgumentException("The selected time slot is not available.");
        }
        
        if (tableNumber == null || tableNumber <= 0)
        {
            throw new ArgumentException("Table number must be greater than zero.");
        }
    
        if (reservationDate.Date < DateTime.Now.Date)
        {
            throw new ArgumentException("Reservation date cannot be in the past.");
        }
        if (timeSlot.Date < DateTime.Now.Date)
        {
            throw new ArgumentException("Time slot cannot be in the past.");
        }
        if (timeSlot.Hour < 10 || timeSlot.Hour > 22)
        {
            throw new ArgumentException("Time slot must be between 10 AM and 10 PM.");
        }
        if (partySize < 1|| partySize > 10)
        {
            throw new ArgumentException("Party size must be between 1 and 10.");
        }
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty.");
        }
    }
}