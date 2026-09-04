using MediBook.Application.DTOs.Doctors;
using MediBook.Application.DTOs.DoctorSpecializations;
using MediBook.Application.DTOs.DoctorWorkingHours;
using MediBook.Application.Services;
using MediBook.Infrustructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class DoctorsController(IDoctorService doctorService , IDoctorSpecializationService doctorSpecializationService , IDoctorClinicAssignmentService doctorClinicAssignmentService , IDoctorWorkingHourService doctorWorkingHourService) : ApiBaseController
    {
        private readonly IDoctorService _doctorService = doctorService;
        private readonly IDoctorSpecializationService _doctorSpecializationService = doctorSpecializationService;
        private readonly IDoctorClinicAssignmentService _doctorClinicAssignmentService = doctorClinicAssignmentService;
        private readonly IDoctorWorkingHourService _doctorWorkingHourService = doctorWorkingHourService;

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(Guid id , CancellationToken cancellationToken = default)
        {
            var result = await _doctorService.GetDoctorByIdAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return Ok(result.Value);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddDoctor(DoctorRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _doctorService.AddDoctorAsync(request, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return CreatedAtAction(nameof(GetDoctorById), new { id = result.Value }, null);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("{doctorId}/specialties")]
        public async Task<IActionResult> AssignSpecialtyToDoctor(Guid doctorId, AssignSpecialtyToDoctorRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _doctorSpecializationService.AssignSpecialtyToDoctorAsync(doctorId ,request ,cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctorId }, null);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{doctorId}/clinic-locations/{clinicLocationId}")]
        public async Task<IActionResult> AssignDoctorToLocation(Guid doctorId, Guid clinicLocationId, CancellationToken cancellationToken = default)
        {
            var result = await _doctorClinicAssignmentService.AssignDoctorToLocationAsync(doctorId, clinicLocationId, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctorId }, null);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost("{doctorId}/working-hours")]
        public async Task<IActionResult> AddWorkingHour(Guid doctorId, AddWorkingHourRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _doctorWorkingHourService.AddWorkingHourAsync(doctorId, request, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctorId }, null);
        }

        [AllowAnonymous]
        [HttpGet("{id}/working-hours")]
        public async Task<IActionResult> GetWorkingHours(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _doctorService.GetDoctorWithWorkingHoursAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return Ok(result.Value);
        }


    }
}
