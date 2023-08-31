public partial class Sword : Weapon3D
{
	private Node3D _weaponMesh;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_weaponMesh = GetNode<Node3D>("Model");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector3 meshPosition = _weaponMesh.Position;

		meshPosition.X = WeaponBobX;
		meshPosition.Z = WeaponBobZ;
		_weaponMesh.Position = meshPosition;
	}
}
