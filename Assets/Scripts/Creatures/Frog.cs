using UnityEngine;
using System;
using System.Collections.Generic;

public class Frog : MobileCreature
{
	const float				MovementDelay = 3.0f;
	private float			currentMovementDelay;
	private FrogController	frogController;
	
	public Frog()
	{
		this.Speed = 250.0f;
		this.currentMovementDelay = Frog.MovementDelay;
	}
	
	public void SetUp(GameManager gameManager, MazeController mazeController)
	{
		base.SetUp(mazeController);
		
		//this.Visible = false;
		this.Dead = false;

		this.frogController = new FrogController(this, gameManager, mazeController);		
		this.Controller = this.frogController;
	}
	
	public override void Update()
	{
		// Decrement movement delay and don't move if the delay hasn't expired
		if (this.currentMovementDelay > 0.0f)
		{
			float delta = RealTime.deltaTime;
			if (delta > 0.1f) { delta = 0.1f; }
			
			this.currentMovementDelay -= delta;
			if (this.currentMovementDelay > 0.0f)
			{
				return;
			}
		}
		
		if (this.CurrentDirection != SerpentConsts.Dir.None)
		{
			UpdatePosition();
			return;			
		}
		
		this.frogController.Hop();
	}
	
	public override void StartMoving(SerpentConsts.Dir direction)
	{
		base.StartMoving(direction);
		// rotate in direction of movement.
		this.transform.eulerAngles = SerpentConsts.RotationVector3[ (int)direction ];
	}
	
	public override void ArrivedAtDestination(float remainingDisplacement)
	{
		// TODO If the frog has moved off-screen then kill it.
		
		this.currentMovementDelay = Frog.MovementDelay;
		this.CurrentDirection = SerpentConsts.Dir.None; // not currently moving but should we really be resetting direction?
		
		base.ArrivedAtDestination(remainingDisplacement);
	}
	
	// Frogs hop over maze walls so their motion is NEVER blocked.
	protected override bool IsMotionBlocked( SerpentConsts.Dir direction )
	{
		return false;
	}
	
	#region Eating eggs
	
	public override bool TestForInteraction(Creature otherCreature)
	{
		if (!(otherCreature is Egg)) { return false; }
		
		if (this.TouchesCreature(otherCreature))
		{
			// make it die.
			otherCreature.Die();
			return true;
		}
		    
		 return false;
	}
		    
	#endregion Eating eggs
		
	#region Dying
	
	override public void Die()
	{
		// frog are totally destroyed on death
		base.Die();
		
		Destroy(this.gameObject);
		Destroy(this);
	}
	
	#endregion Dying
}