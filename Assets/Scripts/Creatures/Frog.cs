using UnityEngine;
using System;
using System.Collections.Generic;
using Serpent;

public class Frog : MobileCreature
{	
	[SerializeField] private AudioSource		bounceSound;
	
	private float			jumpingDelay;
	private FrogController	frogController;
	
	public bool Jumping { get; private set; }
	
	public void Start()
	{
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		this.Speed = difficulty.FrogJumpingSpeed;
		this.jumpingDelay = difficulty.FrogJumpingDelay;
		CreateFutureJumpEvent();
	}
	
	public void SetUp(GameManager gameManager, MazeController mazeController)
	{	
		base.SetUp(mazeController);
		
		this.Dead = false;

		this.frogController = new FrogController(this, gameManager, mazeController);		
		this.Controller = this.frogController;
	}
	
	public override void StartMoving(Direction direction)
	{
		base.StartMoving(direction);
		// rotate in direction of movement.
		this.transform.eulerAngles = SerpentConsts.RotationVector3[ (int)direction ];
		
		this.bounceSound.Play();
	}
	
	public override void ArrivedAtDestination(float remainingDisplacement)
	{
		// TODO If the frog has moved off-screen then kill it.
		CreateFutureJumpEvent();
		
		this.CurrentDirection = Direction.None; // not currently moving but should we really be resetting direction?
		
		base.ArrivedAtDestination(remainingDisplacement);
		this.Jumping = false;
	}
	
	private void CreateFutureJumpEvent()
	{
		Managers.GameClock.RegisterEvent(this.jumpingDelay, () => DoJump(), EventIdentifier.FrogJump );
	}
	
	private void DoJump()
	{
		this.frogController.Hop();
		this.Jumping = true;	
	}
	
	// Frogs hop over maze walls so their motion is NEVER blocked.
	protected override bool IsMotionBlocked( Direction direction )
	{
		return false;
	}
	
	#region Eating eggs
	
	public override InteractionState TestForInteraction(Creature otherCreature)
	{
		if (this.Jumping) { return InteractionState.Nothing; }
		
		if (!(otherCreature is Egg)) { return InteractionState.Nothing; }
		
		if (this.TouchesCreature(otherCreature))
		{
			// make it die.
			otherCreature.Die();
			return InteractionState.KilledSomething;
		}
		    
		 return InteractionState.Nothing;
	}
		    
	#endregion Eating eggs
		
	#region Dying
	
	override public void Die()
	{
		// frog are totally destroyed on death
		base.Die();
		
		Destroy(this.gameObject);
		Destroy(this);
		
		// Remove jump event
		Managers.GameClock.RemoveEvents(EventIdentifier.FrogJump);
	}
	
	#endregion Dying
}