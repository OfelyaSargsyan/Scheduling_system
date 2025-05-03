namespace Kairos.Shared.Data;
using System.Text.Json.Serialization;
public class Certificate
{
	public long Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Airline { get; set; } = string.Empty;
	[JsonIgnore]
	public ICollection<Employee> Employees { get; set; } = [];
}