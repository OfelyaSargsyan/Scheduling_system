using System.ComponentModel.DataAnnotations;
namespace Kairos.Shared.Data;

public class ShiftHistory
{
	public long Id { get; set; }
	public Employee Employee { get; set; } = null!;
	public Shift Shift { get; set; } = null!;
}

