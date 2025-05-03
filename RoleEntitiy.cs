using System.Text.Json.Serialization;

namespace Kairos.Shared.Data;

public class RoleEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
	[JsonIgnore]
	public UserEntity User { get; set; }

}