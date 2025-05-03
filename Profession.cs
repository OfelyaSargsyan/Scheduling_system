using System.Text.Json.Serialization;

namespace Kairos.Shared.Data;

public class Profession
{
    public long Id { get; set; }
    public string Name { get; set; }= string.Empty;
	[JsonIgnore]
	public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
