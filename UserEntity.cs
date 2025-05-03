namespace Kairos.Shared.Data;

public class UserEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
	public ICollection<RoleEntity> Roles { get; set; } = [];
	public bool IsEditing { get; set; } = false;
	public string RolesAsString
	{
		get => Roles.Any() ? Roles.First().Name : ""; 
		set
		{
			Roles.Clear();
			Roles.Add(new RoleEntity { Name = value }); 
		}
	}
}

