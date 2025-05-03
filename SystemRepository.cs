using Kairos.Shared.Data;
using Kairos.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kairos.WebAPI.Data.Repositories;

public class SystemRepository : ISystemRepository
{
	private readonly AppDbContext _context;
	public SystemRepository(AppDbContext context)
	{
		_context = context;
	}


	public async Task<List<Employee>> GetEmployeesAsync()
		=> await _context.Employees
				.Include(e => e.Professions)
				.Include(e => e.Certificates)
				.ToListAsync();

	public async Task AddEmployeeAsync(Employee employee)
	{
		var existingProfession = await _context.Professions.Where(c => employee.Professions.Select(r => r.Id).Contains(c.Id)).ToListAsync();
		if (existingProfession == null)
		{
			throw new InvalidOperationException("The specified profession does not exist.");
		}
		var certificates = await _context.Certificates.Where(c => employee.Certificates.Select(r => r.Id).Contains(c.Id)).ToListAsync();

		employee.Professions = existingProfession;
		employee.Certificates = certificates;
		await _context.Employees.AddAsync(employee);
		await _context.SaveChangesAsync();
	}


	public async Task UpdateEmployeeAsync(Employee employee)
	{
		var existingEmployee = await _context.Employees
	   .Include(e => e.Professions)
	   .Include(e => e.Certificates)
	   .FirstOrDefaultAsync(e => e.Id == employee.Id);

		if (existingEmployee == null)
		{
			throw new InvalidOperationException("The specified employee does not exist.");
		}

		var existingProfessions = await _context.Professions
		.Where(p => employee.Professions.Select(pr => pr.Id).Contains(p.Id))
		.ToListAsync();

		existingEmployee.Professions.Clear();
		foreach (var profession in existingProfessions)
		{
			existingEmployee.Professions.Add(profession);
		}

		var certificates = await _context.Certificates
			.Where(c => employee.Certificates.Select(r => r.Id).Contains(c.Id))
			.ToListAsync();

		existingEmployee.Certificates.Clear();
		foreach (var certificate in certificates)
		{
			existingEmployee.Certificates.Add(certificate);
		}

		existingEmployee.Name = employee.Name;
		existingEmployee.Surname = employee.Surname;
		existingEmployee.Email = employee.Email;
		existingEmployee.IsAvailable = employee.IsAvailable;

		await _context.SaveChangesAsync();
	}


	public async Task<bool> DeleteEmployeeAsync(long employeeId)
	{
		var employee = await _context.Employees
			.FirstOrDefaultAsync(e => e.Id == employeeId);

		if (employee == null)
		{
			return false;
		}

		_context.Employees.Remove(employee);

		await _context.SaveChangesAsync();
		return true;
	}

	public async Task<List<Profession>> GetProfessionsAsync()
	{
		var professions = await _context.Professions.ToListAsync();
		return professions;
	}

	public async Task AddProfessionAsync(Profession profession)
	{
		await _context.Professions.AddAsync(profession);
		await _context.SaveChangesAsync();
	}
	public async Task UpdateProfessionAsync(Profession profession)
	{
		_context.Professions.Update(profession);
		await _context.SaveChangesAsync();
	}
	public async Task<bool> DeleteProfessionAsync(long professionId)
	{
		var profession = await _context.Professions.FindAsync(professionId);
		if (profession == null)
		{
			return false;
		}
		_context.Professions.Remove(profession);
		await _context.SaveChangesAsync();
		return true;
	}
	public async Task<List<Certificate>> GetCertificatesAsync()
	{
		return await _context.Certificates.ToListAsync();
	}


	public async Task AddCertificateAsync(Certificate certificate)
	{
		await _context.Certificates.AddAsync(certificate);
		await _context.SaveChangesAsync();
	}

	public async Task UpdateCertificateAsync(Certificate certificate)
	{
		_context.Certificates.Update(certificate);
		await _context.SaveChangesAsync();
	}

	public async Task<bool> DeleteCertificateAsync(long certificateId)
	{
		var certificate = await _context.Certificates.FindAsync(certificateId);
		if (certificate == null)
		{
			return false;
		}
		_context.Certificates.Remove(certificate);
		await _context.SaveChangesAsync();
		return true;
	}

	public async Task<List<Shift>> GetAllShiftsAsync()
	{
		return await _context.Shifts.ToListAsync();
	}

	//-------------------------------------//
	public async Task ReassignShiftsForNewDayAsync()
	{
		var shifts = await _context.Shifts.OrderBy(s => s.Id).ToListAsync();
		if (!shifts.Any())
			throw new Exception("No shifts available");

		var now = DateTime.UtcNow;
		var lastUpdate = DateTime.UtcNow.Date.AddMinutes(-5);

		var shiftHistorySinceLastUpdate = await _context.ShiftHistorys
			.Include(sh => sh.Employee)
			.ToListAsync();

		if (!shiftHistorySinceLastUpdate.Any())
		{
			Console.WriteLine("No shifts found since last update.");
			return;
		}

		foreach (var history in shiftHistorySinceLastUpdate)
		{
			var currentShiftIndex = shifts.FindIndex(s => s.Id == history.Shift.Id);
			var nextShift = shifts[(currentShiftIndex + 1) % shifts.Count];

			history.Shift = nextShift;
		}

		await _context.SaveChangesAsync();
		Console.WriteLine("Shifts reassigned without adding new rows.");
	}


	public async Task SaveDailyWorkHistoryAsync()
	{
		var recentShiftHistory = await _context.ShiftHistorys
			.Include(sh => sh.Employee)
			.Include(sh => sh.Shift)
			.ToListAsync();

		foreach (var history in recentShiftHistory)
		{
			_context.WorkHistory.Add(new WorkHistory
			{
				Employee = history.Employee,
				Shift = history.Shift,
				Date = DateTime.UtcNow,
			});
		}

		await _context.SaveChangesAsync();
		Console.WriteLine("Work history saved for the interval.");
	}
	public async Task GenerateFutureShiftHistoriesAsync()
	{
		var shifts = await _context.Shifts.OrderBy(s => s.Id).ToListAsync();
		if (!shifts.Any())
		{
			return;
		}

		var employees = await _context.Employees.ToListAsync();
		if (!employees.Any())
		{
			return;
		}

		var today = DateTime.UtcNow.Date;

		var oldestDate = today.AddDays(-1);
		var outdatedEntries = await _context.FutureShiftHistories
			.Where(fsh => fsh.Date == oldestDate)
			.ToListAsync();

		if (outdatedEntries.Any())
		{
			_context.FutureShiftHistories.RemoveRange(outdatedEntries);
			await _context.SaveChangesAsync();
			Console.WriteLine($"Removed outdated shift records for {oldestDate}");
		}

		var futureDates = Enumerable.Range(0, 7).Select(offset => today.AddDays(offset)).ToList();

		foreach (var employee in employees)
		{
			var lastShiftHistory = await _context.ShiftHistorys
				.Where(sh => sh.Employee.Id == employee.Id)
				.OrderByDescending(sh => sh.Id)
				.FirstOrDefaultAsync();

			var currentShiftIndex = lastShiftHistory != null
				? shifts.FindIndex(s => s.Id == lastShiftHistory.Shift.Id)
				: -1;

			foreach (var date in futureDates)
			{
				var existingHistory = await _context.FutureShiftHistories
					.FirstOrDefaultAsync(fsh => fsh.Employee.Id == employee.Id && fsh.Date == date);

				if (existingHistory == null)
				{
					var nextShift = shifts[(currentShiftIndex + 1) % shifts.Count];
					currentShiftIndex = (currentShiftIndex + 1) % shifts.Count;

					var futureShiftHistory = new FutureShiftHistory
					{
						Date = date,
						Employee = employee,
						Shift = nextShift
					};

					await _context.FutureShiftHistories.AddAsync(futureShiftHistory);
				}
			}
		}

		await _context.SaveChangesAsync();
		Console.WriteLine("Future shift histories updated successfully.");
	}


	
	public async Task<List<Employee>> GetFutureEmployeesByShiftAsync(long shiftId, DateTime date)
	{
		var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

		return await _context.FutureShiftHistories
			.Where(fsh => fsh.Shift.Id == shiftId && fsh.Date.Date == utcDate.Date)
			.Include(fsh => fsh.Employee)
				.ThenInclude(e => e.Professions)
			.Include(fsh => fsh.Employee.Certificates)
			.Select(fsh => fsh.Employee)
			.ToListAsync();
	}


	public async Task<List<Employee>> GetEmployeesByShiftAsync(long shiftId, DateTime date)
	{
		var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

		return await _context.ShiftHistorys
			.Where(sh => sh.Shift.Id == shiftId)
			.Include(sh => sh.Employee)
				.ThenInclude(e => e.Professions)
			.Include(sh => sh.Employee.Certificates)
			.Select(sh => sh.Employee)
			.ToListAsync();
	}

	public async Task<List<ShiftHistory>> GetShiftHistoriesAsync()
	{
		return await _context.ShiftHistorys
			.Include(sh => sh.Employee)
			.Include(sh => sh.Shift)
			.ToListAsync();
	}

	public async Task AddOrUpdateShiftHistoryAsync(ShiftHistory shiftHistory)
	{
		if (shiftHistory == null || shiftHistory.Employee == null || shiftHistory.Shift == null)
			throw new ArgumentException("ShiftHistory, Employee or Shift cannot be null.");

		var existingShiftHistory = await _context.ShiftHistorys
			.FirstOrDefaultAsync(sh => sh.Employee.Id == shiftHistory.Employee.Id);

		if (existingShiftHistory != null)
		{
			existingShiftHistory.Shift = await _context.Shifts.FindAsync(shiftHistory.Shift.Id)
										 ?? throw new Exception("Shift not found.");
		}
		else
		{
			shiftHistory.Employee = await _context.Employees.FindAsync(shiftHistory.Employee.Id)
									 ?? throw new Exception("Employee not found.");
			shiftHistory.Shift = await _context.Shifts.FindAsync(shiftHistory.Shift.Id)
								 ?? throw new Exception("Shift not found.");

			await _context.ShiftHistorys.AddAsync(shiftHistory);
		}  

		await _context.SaveChangesAsync(); 
	}

	public async Task<List<EmployeeDayOff>> GetEmployeeDayOffsAsync()
	{
		return await _context.EmployeeDayOffs
			.Include(e => e.Employee)
			.ToListAsync();
	}

	public async Task AddOrUpdateEmployeeDayOffsAsync(List<EmployeeDayOff> dayOffs)
	{
		if (dayOffs == null || !dayOffs.Any())
			throw new ArgumentException("The dayOffs list cannot be null or empty.");

		var groupedDayOffs = dayOffs.GroupBy(d => d.Employee.Id);

		foreach (var group in groupedDayOffs)
		{
			var employeeId = group.Key;
			var newDayOfWeeks = group.Select(d => d.DayOfWeek).ToList();

			var existingDayOffs = await _context.EmployeeDayOffs
				.Where(e => e.Employee.Id == employeeId)
				.ToListAsync();

			var daysToRemove = existingDayOffs
				.Where(e => !newDayOfWeeks.Contains(e.DayOfWeek))
				.ToList();
			_context.EmployeeDayOffs.RemoveRange(daysToRemove);

			var daysToAdd = group
				.Where(d => !existingDayOffs.Any(e => e.DayOfWeek == d.DayOfWeek))
				.ToList();

			foreach (var day in daysToAdd)
			{
				day.Employee = await _context.Employees.FindAsync(employeeId)
								 ?? throw new Exception($"Employee with ID {employeeId} not found.");
			}

			_context.EmployeeDayOffs.AddRange(daysToAdd);
		}

		await _context.SaveChangesAsync();
	}

	public async Task AddFlightAsync(Flight flight)
	{
		flight.ProgrammedTime = DateTime.SpecifyKind(flight.ProgrammedTime, DateTimeKind.Utc);

		await _context.Flights.AddAsync(flight);
		await _context.SaveChangesAsync();
	}


	public async Task<List<Flight>> GetAllFlightsAsync()
	{
		return await _context.Flights.ToListAsync();
	}

	public async Task<List<Flight>> GetFlightsForWeekAsync(DateTime startDate, DateTime endDate)
	{
		return await _context.Flights
			.Where(f => f.ProgrammedTime >= startDate && f.ProgrammedTime <= endDate)
			.ToListAsync();
	}

	//Scheduling

	public async Task<CertificationCheck> CheckEmployeeCertificationAsync(long employeeId, string airlineName)
	{
		var employee = await _context.Employees
			.Include(e => e.Certificates)
			.FirstOrDefaultAsync(e => e.Id == employeeId);

		if (employee == null)
		{
			return new CertificationCheck
			{
				IsCertified = false,
				Message = "Employee not found."
			};
		}

		if (!employee.Certificates.Any())
		{
			return new CertificationCheck
			{
				IsCertified = true,
				Message = "Employee has no certificate restrictions and can work for any airline."
			};
		}

		var hasMatchingCertificate = employee.Certificates.Any(c =>
			c.Airline.Split(',')
				.Select(a => a.Trim())
				.Contains(airlineName, StringComparer.OrdinalIgnoreCase));

		return new CertificationCheck
		{
			IsCertified = hasMatchingCertificate,
			Message = hasMatchingCertificate
				? "Employee is certified for this airline."
				: $"Certification mismatch: Employee is not certified for airline {airlineName}."
		};
	}

	public async Task<List<Employee>> GetEmployeesWithCertificationIssuesAsync()
	{
		var employees = await _context.Employees
			.Include(e => e.Certificates)
			.ToListAsync(); 

		var employeesWithIssues = employees
			.Where(e => e.Certificates.Any() &&
						!e.Certificates.Any(c =>
							c.Airline.Split(',')
							.Select(a => a.Trim())
							.Contains(e.Certificates.First().Airline, StringComparer.OrdinalIgnoreCase)))
			.ToList();

		return employeesWithIssues;
	}



	private static readonly Dictionary<long, bool> _employeeNotifications = new();

	public async Task MarkEmployeeAsNotifiedAsync(long employeeId)
	{
		if (!_employeeNotifications.ContainsKey(employeeId))
		{
			_employeeNotifications[employeeId] = true;
		}
	}

	public async Task AddWorkScheduleAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, long employeeId, long shiftId)
	{
		var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

		var employee = await _context.Employees.FindAsync(employeeId);
		var shift = await _context.Shifts.FindAsync(shiftId);

		if (employee == null || shift == null)
		{
			throw new Exception("Employee or Shift not found.");
		}

		var workSchedule = new WorkSchedule
		{
			Date = utcDate, 
			StartTime = startTime,
			EndTime = endTime,
			Employee = employee,
			Shift = shift
		};

		_context.WorkSchedules.Add(workSchedule);
		await _context.SaveChangesAsync();
	}

	public async Task<List<WorkScheduleDto>> GetAllWorkScheduleDtosAsync()
	{
		return await _context.WorkSchedules
			.Select(ws => new WorkScheduleDto
			{
				Id = ws.Id,
				Date = ws.Date,
				StartTime = ws.StartTime,
				EndTime = ws.EndTime,
				EmployeeId = ws.Employee.Id,
				ShiftId = ws.Shift.Id
			})
			.ToListAsync();
	}

	public async Task DeleteWorkScheduleAsync(long employeeId, DateTime date, TimeSpan startTime)
	{
		var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc); 

		var workSchedule = await _context.WorkSchedules
			.FirstOrDefaultAsync(ws => ws.Employee.Id == employeeId
									&& ws.Date.Date == utcDate.Date
									&& ws.StartTime == startTime);

		if (workSchedule != null)
		{
			_context.WorkSchedules.Remove(workSchedule);
			await _context.SaveChangesAsync();
		}
	}



}
