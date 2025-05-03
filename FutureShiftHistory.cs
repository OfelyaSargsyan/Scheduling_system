using System.ComponentModel.DataAnnotations;

namespace Kairos.Shared.Data;

public class FutureShiftHistory
{
	public long Id { get; set; }

	[Required]
	public DateTime Date { get; set; } 

	[Required]
	public Employee Employee { get; set; } = null!;

	[Required]
	public Shift Shift { get; set; } = null!; 
}

