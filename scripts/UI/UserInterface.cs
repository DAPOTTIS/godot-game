public partial class UserInterface : Control
{
	private CharacterBody3D _player;

	private Label _velocityLabel;
	private Label _speedLabel;
	private Weapon3D _rocketLauncher;
	
	
	public override void _Ready()
	{
		_player = GetParent<CharacterBody3D>();
		_velocityLabel = GetNode<Label>("DirectionLabel");
		_speedLabel = GetNode<Label>("SpeedLabel");
		//_rocketLauncher = _player.GetNode<Weapon3D>("Head/Camera3D/Weapon/rocketlauncher");
	}
	
	public override void _Process(double delta)
	{
		//_velocityLabel.Text = $"{_rocketLauncher.CurrentAmmo} / {_rocketLauncher.MaxPool}";
		_speedLabel.Text = $"{Math.Sqrt(_player.Velocity.X * _player.Velocity.X + _player.Velocity.Z * _player.Velocity.Z)}";
	}
	
}
