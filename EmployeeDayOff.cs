using System.ComponentModel.DataAnnotations;
namespace Kairos.Shared.Data;

public class EmployeeDayOff
{
	public long Id { get; set; }

	[Required]
	public Employee Employee { get; set; } = null!; 
	[Required]
	public DayOfWeek DayOfWeek { get; set; } 
}

