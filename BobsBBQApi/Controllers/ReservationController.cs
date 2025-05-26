using BobsBBQApi.BE;
using BobsBBQApi.BLL.Interfaces;
using BobsBBQApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;
[Route("api/[controller]")] 
public class ReservationController : Controller
{
   private readonly IReservationLogic _reservationLogic;
   public ReservationController(IReservationLogic reservationLogic)
   {
       _reservationLogic = reservationLogic;
   }
   [HttpPost("ReserveTable")]
    public IActionResult ReserveTable([FromBody] ReserveTableDto dto)
    {
        if (dto == null)
        {
            MonitorService.Log.Warning("Invalid table data received");
            return BadRequest("Invalid table data.");
        }
        if (!ModelState.IsValid)
        {
            MonitorService.Log.Warning("Model state is invalid");
            return BadRequest(ModelState);
        }
        DateTime reservationDate = dto.ReservationDate;
        int timeSlot = dto.TimeSlot;
        int partySize = dto.PartySize;
        string note = dto.Note ?? string.Empty;
        Guid userId = dto.UserId;
        
            using var activity = MonitorService.ActivitySource.StartActivity("Reserve table controller");
            activity?.SetTag("reservation.date", reservationDate);
            activity?.SetTag("reservation.timeSlot", timeSlot);
            activity?.SetTag("reservation.partySize", partySize);
            if (!string.IsNullOrWhiteSpace(note))
            {
                activity?.SetTag("reservation.note", note);
            }
            activity?.SetTag("reservation.userId", userId);
            MonitorService.Log.Information("Received reservation request: {@ReservationDate}, {@TimeSlot}, {@PartySize}, {@Note}, {@UserId}", 
                reservationDate, timeSlot, partySize, note, userId);
         if (reservationDate == default || timeSlot == default || partySize <= 0 || partySize > 10 || userId == Guid.Empty )
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
              MonitorService.Log.Error(ex, "Error creating reservation: {@ReservationDate}, {@TimeSlot}, {@PartySize}, {@Note}, {@UserId}", 
                  reservationDate, timeSlot, partySize, note, userId);
              return StatusCode(500, "Error creating reservation: " + ex.Message);
         }
    }
    
    [HttpPost("GetAvailableTimeSlots")]
    public IActionResult GetAvailableTimeSlots([FromBody] GetAvailableTimeSlotDto dto)
    {
        using var activity = MonitorService.ActivitySource.StartActivity("GetAvailableTimeSlots called from controller");
        if (dto == null)
        {
            MonitorService.Log.Warning("Invalid timeslot data received");
            return BadRequest("Invalid table data.");
        }
        if (!ModelState.IsValid)
        {
            MonitorService.Log.Warning("Model state is invalid");
            return BadRequest(ModelState);
        }
        DateTime date = dto.Date;
        int partySize = dto.PartySize;
        
        activity?.SetTag("reservation.date", date);
        activity?.SetTag("reservation.partySize", partySize);
        MonitorService.Log.Information("Received request for available time slots: {@Date}, {@PartySize}", date, partySize);
        if (partySize <= 0 || partySize > 10)
        {
            MonitorService.Log.Warning("{@PartySize} is not a valid party size", partySize);
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
            return Ok(availableTimeSlots
                .Select(t => t.ToString("HH"))); 
        }
        catch (Exception ex)
        {
            MonitorService.Log.Error(ex, "Error retrieving available time slots: {@Date}, {@PartySize}", date, partySize);
            return StatusCode(500, "Error retrieving available time slots: " + ex.Message);
        }
    }

}