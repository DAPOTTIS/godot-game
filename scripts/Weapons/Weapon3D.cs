using Vector3 = Godot.Vector3;

[GlobalClass]
public partial class Weapon3D : RigidBody3D
{
	private Area3D _tip;
	public AudioStreamPlayer FireSound;
	
	//weapon bobbing variables
	private float _bobCycle = 0.6f;
	private float _bobup = 0.5f;
	private float _bobFactor = 0.02f;
	private float _bob;
	private double _bobtime;
	private double _cycle;
	protected Vector3 PlayerVelocity;
	protected static float WeaponBobX;
	protected static float WeaponBobZ;
	
	//weapon stuff
	[Export] public int Mag;
	[Export] public int MaxPool;
	[Export] public double PrimShootTimer = 0.5f;
	[Export] public double SecShootTimer = 1.5f;
	
	
	public int CurrentAmmo;
	public int FiredAmmo;

	enum TrigFunc
	{
		Sin, Cos
	}

	public override void _Ready()
	{
		OnReady();
		FireSound = GetNodeOrNull<AudioStreamPlayer>("FireSound");
	}

	public virtual void OnReady()
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		WeaponBobX = CalcBob(0.65f, TrigFunc.Sin);
		WeaponBobZ = CalcBob(0.15f, TrigFunc.Cos);
	}

	
	
	public override void _Input(InputEvent @event)
	{
		
	}

	public virtual void PrimaryFire()
	{
		
	}

	public virtual void SecondaryFire()
	{
		
	}

	private float CalcBob(float freqIn, TrigFunc trig)
	{
		double delta = GetPhysicsProcessDeltaTime();

		if (PlayerVelocity.Length() > 0)
			_bobtime += delta * freqIn;
		//else if(PlayerVelocity==Vector3.Zero)
			//_bobtime = 0;

		_cycle = _bobtime - (int)(_bobtime / _bobCycle) * _bobCycle;
		_cycle /= _bobCycle;
		
		if (_cycle < _bobup)
			_cycle = (float)Math.PI * _cycle / _bobup;
		else
			_cycle = (float)Math.PI + (float)Math.PI * (_cycle - _bobup) / (1.0f - _bobup);

		_bob = (float)Math.Sqrt(PlayerVelocity.X * PlayerVelocity.X + PlayerVelocity.Z * PlayerVelocity.Z) * _bobFactor;
		
		switch (trig)
		{
			case TrigFunc.Sin:
				_bob = (float)(_bob * 0.3 + _bob * Math.Sin(_cycle/2) * Math.Cos(_cycle*2) * 0.15);
				break;
			case TrigFunc.Cos:
				_bob = (float)(_bob * Math.Cos(_cycle)*0.7f);
				break;
		}
		
		//_bob = (float)Math.Clamp(_bob, -0.015, 0.015);
		return _bob;
	}
}
