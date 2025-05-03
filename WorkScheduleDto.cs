namespace Kairos.Shared.Dtos;
	public class WorkScheduleDto
	{
		public long Id { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan StartTime { get; set; }
		public TimeSpan EndTime { get; set; }
		public long EmployeeId { get; set; }
		public long ShiftId { get; set; }
	}

