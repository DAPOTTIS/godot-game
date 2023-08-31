using Godot;
using System;

[GlobalClass]
public class StateMachine : Node
{
	private Dictionary<int, State> States;
	private State CurrentState;
	
	public StateMachine()
	{
		
	}

	public void Add(int key, State state)
	{
		States.Add(key, state);
	}

	public State GetState(int key)
	{
		return States[key];
	}

	public void SetCurrentState(State state)
	{
		CurrentState?.Exit();
		CurrentState = state;
		CurrentState?.Enter();
	}

	public override void _Ready()
	{
		CurrentState?._Ready();
	}

	public override void _PhysicsProcess(double delta)
	{
		CurrentState?._PhysicsProcess();
	}
}

[GlobalClass]
public abstract class State : Node
{
	public virtual void Enter()
	{
		
	}

	public virtual void Exit()
	{
		
	}

	public virtual void _Ready()
	{
		
	}

	public virtual void _PhysicsProcess()
	{
		
	}
}
