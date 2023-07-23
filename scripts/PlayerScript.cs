using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Anotherfailedattempt.scripts;

public partial class PlayerScript : CharacterBody3D
{
	private Vector3 _wishDir = Vector3.Zero;
	private Vector3 _velocity = Vector3.Zero;
	private bool _isJumping;
	private bool _isCrouching;
	private double _primShootTimerCounter;
	private double _secondShootTimerCounter;
	private List<Weapon3D> _weapons = new();
	private Array<Weapon3D> _projectileWeapons = new();

	[Export] private float _gravity = 12f;
	[Export] private float _groundAccel = 10f;
	[Export] private float _groundDecel = 10f;
	[Export] private float _groundMoveSpeed = 8.0f;
	[Export] private float _airMoveSpeed = 1f;
	[Export] private float _jumpVelocity = 4.5f;
	[Export] private float _friction = 6f;
	[Export] private float _airAccel = 1f;
	[Export] private float _camSens = 0.006f;
	
	private Node3D _head;
	private Camera3D _camera;
	private CollisionShape3D _playerCollision;
	private CapsuleShape3D _collShape;
	private RayCast3D _canStand;
	private AudioStreamPlayer _jumpSound;
	private Node3D _weaponSlot;
	private Node3D _inactiveWeapons;
	private Weapon3D _currentWeapon;
	private Node3D _meleeSlot;
	private Node3D _projectileSlot;
	private Weapon3D _rocketLauncher;

	enum CanShootCheck
	{
		Primary, Secondary
	}
	
	//Override methods
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head");
		_camera = GetNode<Camera3D>("Head/Camera3D");
		_playerCollision = GetNode<CollisionShape3D>("PlayerCollision");
		_collShape = (CapsuleShape3D)_playerCollision.Shape;
		_canStand = GetNode<RayCast3D>("CanStand");
		_weaponSlot = GetNode<Node3D>("Head/Camera3D/Weapon");
		_jumpSound = GetNode<AudioStreamPlayer>("JumpSound");
		_inactiveWeapons = GetNode<Node3D>("InactiveWeapon");
		//_meleeSlot = GetNode<Node3D>("InactiveWeapon/melee");
		//_projectileSlot = GetNode<Node3D>("InactiveWeapon/projectile");
		_rocketLauncher = GetNode<Weapon3D>("InactiveWeapon/rocketlauncher");
		InitWeapons();
	}

	private void InitWeapons()
	{
		for (int i = 0; i < _weaponSlot.GetChildCount(); i++)
		{
			var WeaponChild = _weaponSlot.GetChildOrNull<Weapon3D>(i);
			_weapons.Add(WeaponChild);
		}
		SetWeapon(_weapons[0]);
	}

	private void SetWeapon(Weapon3D weapon)
	{
		for (int i = 0; i < _weaponSlot.GetChildCount(); i++)
		{
			_weaponSlot.GetChild<Weapon3D>(i).Visible = false;
		}
		_currentWeapon = weapon;
		_currentWeapon.Visible = true;
	}
	
	private void AddNewWeapon(Weapon3D newWeapon)
	{
		_weapons.Add(newWeapon);
	}
	
	public override void _UnhandledInput(InputEvent @inputEvent)
	{
		if (@inputEvent.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _Input(InputEvent @event)
	{
		Vector3 cameraRot = _camera.Rotation;
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_head.RotateY(-mouseMotion.Relative.X * _camSens);
			_camera.RotateX(-mouseMotion.Relative.Y * _camSens);

			cameraRot.X = Mathf.Clamp(_camera.Rotation.X, Mathf.DegToRad(-80), Mathf.DegToRad(80f));
			_camera.Rotation = cameraRot;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Key1:
					SetWeapon(_weapons[0]);
					break;
				case Key.Key2:
					SetWeapon(_weapons[1]);
					break;
			}
		}
		//_isCrouching = Input.IsActionPressed("crouch");
		
		if (@event.IsActionPressed("primaryfire") && _currentWeapon.CurrentAmmo>0 && CanShoot(_currentWeapon.PrimShootTimer, CanShootCheck.Primary))
		{
			_currentWeapon._PrimaryFire();
			_primShootTimerCounter = 0f;
			_currentWeapon.FireSound.Play();
		}
		if (@event.IsActionPressed("secondaryfire") && _currentWeapon.CurrentAmmo>0 && CanShoot(_currentWeapon.SecShootTimer,CanShootCheck.Secondary))
		{
			_currentWeapon._SecondaryFire();
			_secondShootTimerCounter = 0f;
		}
		SetDir();
	}

	private void Jump()
	{
		if (!IsOnFloor()) return;
		_velocity.Y = _jumpVelocity;
		_jumpSound.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;
		if (IsOnFloor())
		{
			GroundMove();
		}
		else
		{
			AirMove();
		}
		if (Input.IsActionPressed("jump"))
		{
			Jump();
		}
		
		/*(if (MoveAndCollide(_velocity * (float)delta) != null) :(
		{
			var collider = new KinematicCollision3D();
			for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				collider = GetSlideCollision(i);
			}

			var h = collider.GetPosition().Y;
			var playerh = _collShape.Height / 2;
			var diff = playerh - h;
			if (diff < 0)
				diff = 0;
			var playerPos = Position;
			playerPos.Y += diff * 1.25f;
			Position = playerPos;
		}*/
		
		Velocity = _velocity;
		MoveAndSlide();
		
		_primShootTimerCounter += delta;
		_secondShootTimerCounter += delta;
	}
	//Weapon stuff
	private bool CanShoot(double timer, CanShootCheck mode)
	{
		if (mode == CanShootCheck.Primary)
			return _primShootTimerCounter > timer;
		else
			return _secondShootTimerCounter > timer;
	}

	private void SwitchWeaponTo(Weapon3D weapon)
	{
		//var inactiveweapon = inactiveWeapon.GetChildOrNull<Weapon3D>(0);
		//GD.Print(inactiveweapon);
		_currentWeapon = _weaponSlot.GetChildOrNull<Weapon3D>(0);

		//GD.Print(GetNode<Weapon3D>("InactiveWeapon/sword").GetInstanceId());
		//GD.Print(inactiveweapon);
		_weaponSlot.RemoveChild(_currentWeapon);
		//_inactiveWeapons.RemoveChild(inactiveweapon);

		//_currentWeaponSlot.AddChild(inactiveweapon);
		_inactiveWeapons.AddChild(_currentWeapon);
		_currentWeapon = _weaponSlot.GetChildOrNull<Weapon3D>(0);
	}
	//Movement methods
	void Accelerate(float accel, float moveSpeed)
	{
		//acceleration calculated based on desired speed and the dot product of current velocity and wishdir
		float wishSpeed = QNormalize(_wishDir) * moveSpeed;
		float currentSpeed = _velocity.Dot(_wishDir);
		
		float addSpeed = wishSpeed - currentSpeed;
		if (addSpeed <= 0)
			return;

		float accelSpeed = (float)GetPhysicsProcessDeltaTime() * wishSpeed * accel;
		
		if (accelSpeed > addSpeed)
			accelSpeed = addSpeed;
		

		_velocity.X += accelSpeed * _wishDir.X;
		_velocity.Z += accelSpeed * _wishDir.Z;
	}

	private void ApplyFriction()
	{
		var lastSpeed = _velocity.Length();
		
		var control = lastSpeed < _groundDecel ? _groundDecel : lastSpeed;
		var drop = control * _friction * (float)GetPhysicsProcessDeltaTime();
		
		var newSpeed = lastSpeed - drop;
		if (newSpeed < 0)
		{
			newSpeed = 0;
		}

		if (lastSpeed > 0)
		{
			newSpeed /= lastSpeed;
		}
		_velocity.X *= newSpeed;
		_velocity.Z *= newSpeed;
	}

	private void GroundMove()
	{
		Accelerate(_groundAccel, _groundMoveSpeed);
		ApplyFriction();
	}
	
	private void AirMove()
	{
		Accelerate(_airAccel, _airMoveSpeed);
		_velocity.Y -= _gravity * (float)GetPhysicsProcessDeltaTime();
	}

	private void SetDir()
	{
		//setting the direction using input and camera/head rotation
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		_wishDir = (_head.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
	}

	private float QNormalize(Vector3 inputVector)
	{
		//Quake's vector normalize function normalizes the vector and gets the magnitude, so this is replicating it
		inputVector = inputVector.Normalized();
		var result = inputVector.Length();
		return result;
	}
	
}
