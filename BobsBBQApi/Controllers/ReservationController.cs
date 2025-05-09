using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class ReservationController : Controller
{
   private readonly IReservationLogic _reservationLogic;
   public ReservationController(IReservationLogic reservationLogic)
   {
       _reservationLogic = reservationLogic;
   }
   [HttpPost("[action]")]
    public IActionResult ReserveTable(DateTime reservationDate, int timeSlot, int partySize, string note, Guid userId)
    {
            using var activity = MonitorService.ActivitySource.StartActivity("Reserve table controller");
            activity?.SetTag("reservation.date", reservationDate);
            activity?.SetTag("reservation.timeSlot", timeSlot);
            activity?.SetTag("reservation.partySize", partySize);
            activity?.SetTag("reservation.userId", userId);
            MonitorService.Log.Information("Received reservation request: {@reservationDate}, {@timeSlot}, {@partySize}, {@note}, {@userId}", 
                reservationDate, timeSlot, partySize, note, userId);
         if (reservationDate == default || timeSlot == default || partySize <= 0 || partySize > 10 || string.IsNullOrWhiteSpace(note) || userId == Guid.Empty )
         {
             MonitorService.Log.Warning("Invalid reservation request received");
              return BadRequest("All fields are required.");
         }
    
         try
         {
              MonitorService.Log.Information("ReserveTable in ReservationLogic called from controller");
              _reservationLogic.ReserveTable(reservationDate, timeSlot, partySize, note, userId );
              return Ok("Reservation created successfully.");
         }
         catch (Exception ex)
         {
              MonitorService.Log.Error(ex, "Error creating reservation: {@reservationDate}, {@timeSlot}, {@partySize}, {@note}, {@userId}", 
                  reservationDate, timeSlot, partySize, note, userId);
              return StatusCode(500, "Error creating reservation: " + ex.Message);
         }
    }
    
    [HttpPost("[action]")]
    public IActionResult GetAvailableTimeSlots(DateTime date, int partySize)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("GetAvailableTimeSlots called from controller");
        activity?.SetTag("reservation.date", date);
        activity?.SetTag("reservation.partySize", partySize);
        MonitorService.Log.Information("Received request for available time slots: {@date}, {@partySize}", date, partySize);
        if (partySize <= 0 || partySize > 10)
        {
            MonitorService.Log.Warning("{@partySize} is not a valid party size", partySize);
            return BadRequest("Party size must be between 1 and 10.");
        }
        if (date == default)
        {
            MonitorService.Log.Warning("Invalid date received");
            return BadRequest("Date is required.");
        }

        try
        {
            MonitorService.Log.Information("GetAvailableTimeSlot in ReservationLogic called from controller");
            var availableTimeSlots = _reservationLogic.GetAvailableTimeSlot(date, partySize);
            MonitorService.Log.Information("Available time slots retrieved successfully");
            return Ok(availableTimeSlots);
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error retrieving available time slots: {@date}, {@partySize}", date, partySize);
            return StatusCode(500, "Error retrieving available time slots: " + ex.Message);
        }
    }


   
}