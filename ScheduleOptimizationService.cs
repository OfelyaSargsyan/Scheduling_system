using Google.OrTools.Sat;
using Kairos.Shared.Data;
using Kairos.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace Kairos.WebAPI.Services;

public class ScheduleOptimizationService
{
	private readonly AppDbContext _context;

	public ScheduleOptimizationService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<bool> GenerateOptimalScheduleAsync(DateTime targetDate, long shiftId)
	{
		targetDate = DateTime.SpecifyKind(targetDate.Date, DateTimeKind.Utc);

		var shift = await _context.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId);
		if (shift == null) return false;

		var futureShiftHistories = await _context.FutureShiftHistories
			.Include(f => f.Employee)
				.ThenInclude(e => e.Certificates)
			.Include(f => f.Employee)
				.ThenInclude(e => e.Professions)
			.Where(f => f.Date == targetDate && f.Shift.Id == shiftId)
			.ToListAsync();

		if (!futureShiftHistories.Any()) return false;

		var employees = futureShiftHistories.Select(h => h.Employee).Distinct().ToList();

		var flights = await _context.Flights
			.Where(f => f.ProgrammedTime.Date == targetDate &&
						f.ProgrammedTime.TimeOfDay >= shift.StartTime &&
						f.ProgrammedTime.TimeOfDay < shift.EndTime)
			.ToListAsync();

		if (!flights.Any()) return false;

		var existingSchedules = await _context.WorkSchedules
			.Where(w => w.Date == targetDate && w.Shift.Id == shiftId)
			.ToListAsync();

		_context.WorkSchedules.RemoveRange(existingSchedules);

		var usedProfessions = new HashSet<string>();
		var assignedEmployees = new HashSet<long>();

		foreach (var flight in flights)
		{
			foreach (var employee in employees)
			{
				if (assignedEmployees.Contains(employee.Id)) continue;

				var profession = employee.Professions.FirstOrDefault()?.Name?.ToLower() ?? "";

				if (usedProfessions.Contains(profession)) continue;

				bool hasCert = employee.Certificates.Any();
				bool hasValidCert = employee.Certificates
					.Any(c => c.Airline.Split(',').Select(x => x.Trim()).Contains(flight.AirlineName));

				if (hasCert && !hasValidCert) continue;

				await _context.WorkSchedules.AddAsync(new WorkSchedule
				{
					Date = targetDate,
					StartTime = flight.ProgrammedTime.TimeOfDay,
					EndTime = flight.ProgrammedTime.TimeOfDay.Add(TimeSpan.FromMinutes(30)),
					Employee = employee,
					Shift = shift
				});

				assignedEmployees.Add(employee.Id);

				if (!string.IsNullOrWhiteSpace(profession))
					usedProfessions.Add(profession);

				break; 
			}
		}

		await _context.SaveChangesAsync();
		return true;
	}


}
