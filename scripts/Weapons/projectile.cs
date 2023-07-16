using Godot;
using System;

public partial class projectile : RigidBody3D
{
	private void _on_timer_timeout()
	{
		QueueFree();
	}

	public override void _Process(double delta)
	{
		//GD.Print(LinearVelocity);
	}

	private void OnBodyEntered(PhysicsBody3D body)
	{
		var collidingBodies = GetCollidingBodies();
		for (int i = 0; i < collidingBodies.Count; i++)
		{
			
		}
	}
}
