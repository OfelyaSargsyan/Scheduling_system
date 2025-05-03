namespace Kairos.Shared.Data;

public class WorkHistory
{
	public long Id { get; set; }
	public DateTime Date { get; set; }
	public Employee Employee { get; set; } = null!;
	public Shift Shift { get; set; } = null!;
}

