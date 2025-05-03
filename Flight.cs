using System.ComponentModel.DataAnnotations;
namespace Kairos.Shared.Data;

public class Flight
{
	public long Id { get; set; }
	public string FlightNumber { get; set; } = string.Empty;
	public string AirlineName { get; set; } = string.Empty;
	public DateTime ProgrammedTime { get; set; }
	public string FlightStatus { get; set; } = string.Empty;
	public string ArrivalOrDeparture { get; set; } = string.Empty;
}
