using Kairos.Shared.Data;
using Kairos.Shared.Dtos;
using System.Threading.Tasks;

namespace Kairos.WebAPI.Data.Repositories;

public interface ISystemRepository
{
    Task<List<Employee>> GetEmployeesAsync();
    Task AddEmployeeAsync(Employee employee);
    Task UpdateEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(long employeeId);
    Task<List<Profession>> GetProfessionsAsync();
    Task AddProfessionAsync(Profession profession);
    Task UpdateProfessionAsync(Profession profession);
    Task<bool> DeleteProfessionAsync(long professionId);
    Task<List<Certificate>> GetCertificatesAsync();
    Task AddCertificateAsync(Certificate certificate);
    Task UpdateCertificateAsync(Certificate certificate);
    Task<bool> DeleteCertificateAsync(long certificateId);
    Task<List<Shift>> GetAllShiftsAsync();
    Task ReassignShiftsForNewDayAsync();
    Task SaveDailyWorkHistoryAsync();
    Task GenerateFutureShiftHistoriesAsync();
	Task<List<Employee>> GetEmployeesByShiftAsync(long shiftId, DateTime date);
    Task<List<EmployeeDayOff>> GetEmployeeDayOffsAsync();
    Task<List<Employee>> GetFutureEmployeesByShiftAsync(long shiftId, DateTime date);
	Task AddOrUpdateEmployeeDayOffsAsync(List<EmployeeDayOff> dayOffs);
    Task<List<ShiftHistory>> GetShiftHistoriesAsync();
    Task AddOrUpdateShiftHistoryAsync(ShiftHistory shiftHistory);
    Task<List<Employee>> GetEmployeesWithCertificationIssuesAsync();
    Task MarkEmployeeAsNotifiedAsync(long employeeId);
	Task<CertificationCheck> CheckEmployeeCertificationAsync(long employeeId, string airlineName);
    Task AddWorkScheduleAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, long employeeId, long shiftId);
    Task<List<WorkScheduleDto>> GetAllWorkScheduleDtosAsync();
	Task DeleteWorkScheduleAsync(long employeeId, DateTime date, TimeSpan startTime);
    Task AddFlightAsync(Flight flight);
    Task<List<Flight>> GetAllFlightsAsync();
    Task<List<Flight>> GetFlightsForWeekAsync(DateTime startDate, DateTime endDate);

}
