using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.DAL.Repositories.Interfaces;
using BobsBBQApi.Helpers;

namespace BobsBBQApi.BLL;

public class ReservationLogic : IReservationLogic
{
    private readonly IReservationRepository _reservationRepository;
    public ReservationLogic(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }
    public List<DateTime> GetAvailableTimeSlot(DateTime date)
    {
        var reservedSlots = _reservationRepository.GetReservedSlots(date);
        var defaultSlots = DefaultTimeSlots.GetSlots(date);
        var availableTimeSlots = defaultSlots.Where(slots => !reservedSlots.Any(r => r.Hour == slots.Hour)).ToList();
        return availableTimeSlots;
    }
}