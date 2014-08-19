using UnityEngine;
using System;
using System.Collections;
using Serpent;

public class Egg : Creature
{	
	public event Action<Egg> FullyGrown;
	public event Action<Egg> Hatched;
	
	private float growingDuration;
	private float hatchingDuration;
	
	private float grownTime;
	private float hatchingTime;
	
	public bool IsFullyGrown { get; set; }
	private bool shouldHatch = false;
	
	private float shakingDisplacement;
	//private float shakingDisplacementStep;
	private float shakingStepTime;
	private int numShakesRemaining;
	private TweenPosition shakeTween;
	private bool hasShakingBegun;

	Egg()
	{
		this.IsFullyGrown = false;
		this.hasShakingBegun = false;
	}
	
	public void Setup()
	{
		DifficultySettings difficulty = Managers.SettingsManager.GetCurrentSettings();
		if (this.Side == Side.Player)
		{
			this.growingDuration = difficulty.PlayerEggCreationTime;
			this.hatchingDuration = 0.0f; // hatches at end of level
			StartGrowing();
		}
		else
		{
			this.growingDuration = difficulty.EnemyEggCreationTime;
			this.hatchingDuration = difficulty.EnemyEggHatchingTime;
			StartGrowing();
			SetHatchingTime();
		}
	}
	
	public void StartGrowing()
	{
		SetGrownTime();
		Grow();
	}
	
	private void SetGrownTime()
	{
		if (this.Side == Side.Player)
		{
			this.grownTime = Managers.GameClock.Time + this.growingDuration;
		}
		else
		{
			this.grownTime = Managers.GameClock.Time + this.growingDuration;				
		}
	}
	
	public void SetHatchingTime()
	{
		if (this.grownTime == 0.0f)
		{
			SetGrownTime();
		}
		this.hatchingTime = this.grownTime + this.hatchingDuration;
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
		TweenScale scaleTween = TweenScale.Begin(this.gameObject, this.growingDuration, finalScale);
		
		EventDelegate tweenFinished = new EventDelegate(this, "ScaledUp");
		scaleTween.onFinished.Add ( tweenFinished );
	}
	
	private void ScaledUp()
	{
		this.IsFullyGrown = true;
		if (this.FullyGrown != null)
		{
			this.FullyGrown(this);	
		}
	}
	
	// Hatching behavior.  TODO Could this be handled in a different way than polling time?
	void Update()
	{
		if (this.IsFullyGrown && this.shouldHatch && this.Hatched != null)
		{
			if (this.hasShakingBegun == false && Managers.GameClock.Time + SerpentConsts.EggShakeDuration >= this.hatchingTime)
			{
				this.hasShakingBegun = true;
				BeginShaking(SerpentConsts.EggShakeDuration, SerpentConsts.EggNumShakes, SerpentConsts.EggShakeDisplacement);				
			}			
			else if (Managers.GameClock.Time >= this.hatchingTime)
			{
				this.Hatched(this);
				Die();
			}
		}		
	}
	
	override public void Die()
	{
		if (this.shakeTween != null)
		{
			this.shakeTween.enabled = false;
		}
		
		// eggs are totally destroyed on death
		base.Die();
		
		Destroy(this.gameObject);
		Destroy(this);
	}
	
	public void BeginShaking(float totalDuration, int numVibrations, float displacement)
	{
		this.numShakesRemaining = numVibrations;
		this.shakingDisplacement = displacement;				
		this.shakingStepTime = CalculateInitialDuration(totalDuration, numVibrations);
		
		StartCoroutine( StartNextShakeCoroutine() );		
	}
	
	private float CalculateInitialDuration(float totalDuration, int numSteps)
	{
		// Figure out the multipliers on all the steps and add them together
		// Once we know that total, the initial duration is the total duration
		// divided by the total.
		float multiplierTotal = 0.0f;
		float multiplier = 1.0f;
		
		for (int i = 0; i < numSteps; ++i)
		{
			multiplierTotal += multiplier;
			multiplier *= 0.95f;
		}
		
		float initialDuration = totalDuration / multiplierTotal;
		return initialDuration;
	}
	
	private void EndOfShake()
	{
		if (this.shakeTween != null)
		{
			Destroy(this.shakeTween);
			this.shakeTween = null;
		}
	
		this.numShakesRemaining--;
		if (this.numShakesRemaining < 0)
		{
			return;
		}
		else if (this.numShakesRemaining == 0)
		{
			// We need to do one more half animation to get back to the beginning
			this.shakingDisplacement *= 0.5f;
		}
		
		float newDisplacement = this.shakingDisplacement * -1;
		this.shakingDisplacement = newDisplacement;
		
		this.shakingStepTime *= 0.95f;

		// Replace it with the new one
		StartCoroutine( StartNextShakeCoroutine() );
	}
		
	private IEnumerator StartNextShakeCoroutine()
	{
		yield return new WaitForEndOfFrame();
		
		Vector3 pos = this.sprite.cachedTransform.localPosition;
		pos.x += this.shakingDisplacement;
		
		this.shakeTween = TweenPosition.Begin( this.gameObject, this.shakingStepTime, pos );
		EventDelegate tweenFinished = new EventDelegate(this, "EndOfShake");
		this.shakeTween.onFinished.Add ( tweenFinished );
	}
	
}

