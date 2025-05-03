namespace Kairos.Shared.Data;

using System.ComponentModel.DataAnnotations;


public class Shift
{
	public long Id { get; set; } 

	[Required]
	public string Name { get; set; } = string.Empty; 

	[Required]
	public TimeSpan StartTime { get; set; } 

	[Required]
	public TimeSpan EndTime { get; set; } 

    public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
}
