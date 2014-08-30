using UnityEngine;
using System;
using System.Collections.Generic;
using Serpent;

public class Frog : MobileCreature
{
	[SerializeField] private AudioSource		bounceSound;
	
	private float			jumpingDelay;
	private float			currentJumpingDelay;
	private FrogController	frogController;
	
	public void Start()
	{
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		this.Speed = difficulty.FrogJumpingSpeed;
		this.jumpingDelay = difficulty.FrogJumpingDelay;
		this.currentJumpingDelay = this.jumpingDelay;
	}
	
	public void SetUp(GameManager gameManager, MazeController mazeController)
	{	
		base.SetUp(mazeController);
		
		this.Dead = false;

		this.frogController = new FrogController(this, gameManager, mazeController);		
		this.Controller = this.frogController;
	}
	
	public override void Update()
	{
		// Decrement movement delay and don't move if the delay hasn't expired
		// This should be replaced with a game clock event.
		if (this.currentJumpingDelay > 0.0f)
		{
			float delta = RealTime.deltaTime;
			if (delta > 0.1f) { delta = 0.1f; }
			
			this.currentJumpingDelay -= delta;
			if (this.currentJumpingDelay > 0.0f)
			{
				return;
			}
		}
		
		if (this.CurrentDirection != Direction.None)
		{
			UpdatePosition();
			return;			
		}
		
		this.frogController.Hop();
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
		
		this.currentJumpingDelay = this.jumpingDelay;
		this.CurrentDirection = Direction.None; // not currently moving but should we really be resetting direction?
		
		base.ArrivedAtDestination(remainingDisplacement);
	}
	
	// Frogs hop over maze walls so their motion is NEVER blocked.
	protected override bool IsMotionBlocked( Direction direction )
	{
		return false;
	}
	
	#region Eating eggs
	
	public override InteractionState TestForInteraction(Creature otherCreature)
	{
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
	}
	
	#endregion Dying
}