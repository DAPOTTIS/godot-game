global using Godot;
global using System;
global using System.Collections.Generic;

namespace Anotherfailedattempt.scripts;
//@todo: make use of partial class to separate weapon logic and movement logic and whatever
public partial class PlayerScript : CharacterBody3D
{
	private Vector3 _wishDir = Vector3.Zero;
	private Vector3 _velocity = Vector3.Zero;
	private bool _isJumping;
	private bool _isCrouching;
	private double _primShootTimerCounter;
	private double _secondShootTimerCounter;
	private List<Weapon3D> _weapons = new();

	[Export] private float _gravity = 12F;
	[Export] private float _groundAccel = 10F;
	[Export] private float _groundDecel = 10F;
	[Export] private float _groundMoveSpeed = 8.0F;
	[Export] private float _airMoveSpeed = 1F;
	[Export] private float _jumpVelocity = 4.5F;
	[Export] private float _friction = 6F;
	[Export] private float _airAccel = 1F;
	[Export] private float _camSens = 0.006F;
	
	private Node3D _head;
	private Node3D _weaponSlot;
	private Camera3D _camera;
	private CollisionShape3D _playerCollision;
	private CapsuleShape3D _collShape;
	private RayCast3D _canStand;
	private AudioStreamPlayer _jumpSound;
	private Weapon3D _currentWeapon;

	enum CanShootCheck
	{
		Primary, Secondary
	}
	
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
		InitWeapons();
	}
	
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed("ui_cancel")) 
			Input.MouseMode = Input.MouseModeEnum.Visible;
		
		//test
		//Input.MouseMode = inputEvent.IsActionPressed("ui_cancel") ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
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
		
		if (@event is InputEventKey keyEvent)
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
		
		//Crouching is munted so ill do it later
		//_isCrouching = Input.IsActionPressed("crouch");
		
		if (@event.IsActionPressed("primaryfire") && _currentWeapon.CurrentAmmo>0 && CanShoot(_currentWeapon.PrimShootTimer, CanShootCheck.Primary))
		{
			_currentWeapon.PrimaryFire();
			_primShootTimerCounter = 0f;
			_currentWeapon.FireSound?.Play();
		}
		
		if (@event.IsActionPressed("secondaryfire") && _currentWeapon.CurrentAmmo>0 && CanShoot(_currentWeapon.SecShootTimer,CanShootCheck.Secondary))
		{
			_currentWeapon.SecondaryFire();
			_secondShootTimerCounter = 0f;
		}
		
		SetDir();
	}

	private void Jump()
	{
		if (!IsOnFloor()) return;
		_velocity.Y += _jumpVelocity;
		_jumpSound.Play();
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;
		if (IsOnFloor()) 
			GroundMove();
		
		else 
			AirMove();
		
		if (Input.IsActionPressed("jump"))
			Jump();
		
		
		//tried to do auto stair climbing, but sadly godot doesnt seem to support detecting separate faces
		//like what i saw in PhysX docs
		//
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
	
	
	//Weapon Methods
	private bool CanShoot(double timer, CanShootCheck mode)
	{
		if (mode == CanShootCheck.Primary)
			return _primShootTimerCounter > timer;
		return _secondShootTimerCounter > timer;
	}
	
	private void InitWeapons()
	{
		for (int i = 0; i < _weaponSlot.GetChildCount(); i++)
		{
			var weaponChild = _weaponSlot.GetChildOrNull<Weapon3D>(i);
			_weapons.Add(weaponChild);
		}
		
		//temporary hack, sets sword as current weapon when player loads in
		SetWeapon(_weapons[0]);
	}

	private void SetWeapon(Weapon3D weapon)
	{
		//iterates through stored weapons to set them as invisible, then sets current weapon as visible
		
		for (int i = 0; i < _weapons.Count; i++) 
			_weapons[i].Visible = false;
		
		_currentWeapon = weapon;
		_currentWeapon.Visible = true;
	}
	
	private void AddNewWeapon(Weapon3D newWeapon)
	{
		//currently unused
		
		_weapons.Add(newWeapon);
	}
	
	
	//Movement Methods
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
			newSpeed = 0;

		if (lastSpeed > 0)
			newSpeed /= lastSpeed;

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
