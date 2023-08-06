public partial class rocketlauncher : Weapon3D
{
	[Export] private float _projectileForce = 25f;
	
	[Export] private PackedScene _projectileScene;
	private Area3D _tip;
	private Node3D _weaponMesh;
	private CharacterBody3D _player;
	private Label _ammoCounter;
	private AnimationPlayer _animPlayer;

	//private SubViewport _ammoViewport;


	public override void OnReady()
	{
		_tip = GetNode<Area3D>("Tip");
		_weaponMesh = GetNode<Node3D>("Model");
		_player = GetOwner<CharacterBody3D>();
		_ammoCounter = _weaponMesh.GetNode<Label>("%AmmoCounter");
		_animPlayer = _weaponMesh.GetNode<AnimationPlayer>("AnimationPlayer");

		CurrentAmmo = MaxPool;
		_ammoCounter.Text = $"{CurrentAmmo}";
	}
	

	public override void _Process(double delta)
	{
		Vector3 meshPosition = _weaponMesh.Position;

		meshPosition.X = WeaponBobX;
		meshPosition.Z = WeaponBobZ;
		_weaponMesh.Position = meshPosition;
		PlayerVelocity = _player.Velocity;
	}

	public override void PrimaryFire()
	{
		FiredAmmo = 1;
		var projectile = (RigidBody3D)_projectileScene.Instantiate();
		projectile.ApplyImpulse(_tip.GlobalTransform.Basis.X * _projectileForce);
		_tip.AddChild(projectile);
		CurrentAmmo -= FiredAmmo;
		_ammoCounter.Text = $"{CurrentAmmo}";
		_animPlayer.Play("Shoot");
		projectile.TopLevel = true;
	}
	public override void SecondaryFire()
	{
		FiredAmmo = 1;
		//@todo: Secondary firing stuff
		//var projectile = (RigidBody3D)_projectileScene.Instantiate();
		//projectile.ApplyImpulse(_tip.GlobalTransform.Basis.X * _projectileForce);
		//_tip.AddChild(projectile);
		CurrentAmmo -= FiredAmmo;
		_ammoCounter.Text = $"{CurrentAmmo}";
		_animPlayer.Play("Shoot");
	}
	
}
