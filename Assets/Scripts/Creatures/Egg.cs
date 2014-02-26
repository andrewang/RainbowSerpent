using UnityEngine;
using System;

public class Egg : Creature
{	
	public event Action FullyGrown;
	public event Action<Egg> Hatched;
	
	private DateTime grownTime;
	private DateTime hatchingTime;
	private bool fullyGrown = false;

	void Start()
	{
		this.grownTime = DateTime.Now + SerpentConsts.TimeToLayEgg;		
		this.hatchingTime = this.grownTime + SerpentConsts.EnemyEggHatchingTime;
		
		// Begin animation of scaling up
		this.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		Vector3 finalScale = new Vector3(1.0f, 1.0f, 1.0f);
		TweenScale scaleTween = TweenScale.Begin(this.gameObject, (float)SerpentConsts.TimeToLayEgg.TotalSeconds, finalScale);
		
		EventDelegate tweenFinished = new EventDelegate(this, "ScaledUp");
		scaleTween.onFinished.Add ( tweenFinished );
	}
	
	private void ScaledUp()
	{
		this.fullyGrown = true;
		this.FullyGrown();	
	}
	
	// Hatching behavior?
	void Update()
	{
		/*
		if (this.fullyGrown == false)
		{
			if (DateTime.Now > this.grownTime)
			{
				this.fullyGrown = true;
				this.FullyGrown();
			}
		}
		else 
		*/
		if (this.fullyGrown && DateTime.Now > this.hatchingTime)
		{
			this.Hatched(this);
		}		
	}
	
	public void SetSpriteDepth(int depth)
	{
		this.sprite.depth = depth;
	}
}

