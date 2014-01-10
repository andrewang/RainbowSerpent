using UnityEngine;
using System;

public class Egg : Creature
{
	// An egg will have a sprite
	[SerializeField] private UISprite sprite;	
	
	public event Action FullyGrown;
	public event Action<Egg> Hatched;
	
	private DateTime grownTime;
	private DateTime hatchingTime;
	private bool fullyGrown = false;
			
	new public float Radius
	{
		get
		{
			// Average width and height and then halve this average diameter in order to get a radius.
			return (this.sprite.height + this.sprite.width) * 0.25f;
		}
	}
	
	void Start()
	{
		this.grownTime = DateTime.Now + SerpentConsts.TimeToLayEgg;		
		this.hatchingTime = this.grownTime + SerpentConsts.EnemyEggHatchingTime;
		
		// Begin animation of scaling up
		this.transform.localScale = new Vector3(0.01f, 0.01f);
		Vector3 finalScale = new Vector3(1.0f, 1.0f);
		TweenScale.Begin(this.gameObject, (float)SerpentConsts.TimeToLayEgg.TotalSeconds, finalScale);
	}
	
	// Hatching behavior?
	void Update()
	{
		if (this.fullyGrown == false)
		{
			if (DateTime.Now > this.grownTime)
			{
				this.fullyGrown = true;
				this.FullyGrown();
			}
		}
		else if (DateTime.Now > this.hatchingTime)
		{
			this.Hatched(this);
		}		
	}
	
	public void SetSpriteDepth(int depth)
	{
		this.sprite.depth = depth;
	}
}

