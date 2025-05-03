using System.ComponentModel.DataAnnotations;
namespace Kairos.Shared.Data;

public class Employee
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

	[Required]
	[EmailAddress(ErrorMessage = "Please enter a valid email address.")]
	[RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]+$",
			ErrorMessage = "Email must contain @ and a valid domain ending.")]
	public string Email { get; set; }=string.Empty;
    public bool IsAvailable { get; set; }
    public ICollection<Profession> Professions { get; set; } = [];
	public ICollection<Certificate> Certificates { get; set; } = [];
	public ICollection<WorkSchedule> WorkSchedules { get; set; } = [];
}

