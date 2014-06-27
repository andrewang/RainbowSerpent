using UnityEngine;
using System;

public class Egg : Creature
{	
	public SnakeBody ContainingBody
	{
		get; set; 
	}
	
	public event Action<Egg> FullyGrown;
	public event Action<Egg> Hatched;
	
	private float grownTime;
	private float hatchingTime;
	private bool fullyGrown = false;
	private bool shouldHatch = false;

	void Start()
	{
		this.grownTime = Managers.GameClock.Time + SerpentConsts.TimeToLayEgg;		
		this.shouldHatch = false;
		
		Grow();
	}
	
	public void SetHatchingTime(float hatchingTime)
	{
		this.hatchingTime = this.grownTime + hatchingTime;
		this.shouldHatch = true;		
	}
	
	/// <summary>
	/// Make this egg hatch right away (for player eggs at the end of the level).
	/// </summary>
	public void Hatch()
	{
		this.shouldHatch = true;
		this.hatchingTime = 0.0f;
	}
	
	private void Grow()
	{
		// Begin animation of scaling up
		this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		Vector3 finalScale = new Vector3(1.0f, 1.0f, 1.0f);
		TweenScale scaleTween = TweenScale.Begin(this.gameObject, SerpentConsts.TimeToLayEgg, finalScale);
		
		EventDelegate tweenFinished = new EventDelegate(this, "ScaledUp");
		scaleTween.onFinished.Add ( tweenFinished );
	}
	
	private void ScaledUp()
	{
		this.fullyGrown = true;
		this.FullyGrown(this);	
	}
	
	// Hatching behavior.  TODO Could this be handled in a different way than polling time?
	void Update()
	{
		if (this.fullyGrown && this.shouldHatch && Managers.GameClock.Time >= this.hatchingTime)
		{
			this.Hatched(this);
		}		
	}
	
	override public void Die()
	{
		// eggs are totally destroyed on death
		base.Die();
		
		Destroy(this.gameObject);
		Destroy(this);
	}
	
	public void SetSpriteDepth(int depth)
	{
		this.sprite.depth = depth;
	}
}

