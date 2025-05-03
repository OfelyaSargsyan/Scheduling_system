using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kairos.Shared.Data;

public class WorkSchedule
{
	public long Id { get; set; }

	[Required]
	public DateTime Date { get; set; }

	[Required]
	public TimeSpan StartTime { get; set; }

	[Required]
	public TimeSpan EndTime { get; set; }

	public Employee Employee { get; set; } = null!;

	public Shift Shift { get; set; } = null!;
}
