using UnityEngine;
using System;

public class Egg : Creature
{
	// An egg will have a sprite
	[SerializeField] private UISprite sprite;	
	
	public event Action<Egg> EggHatched;
	
	private float timeUntilHatched;
		
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
		this.timeUntilHatched = (float) SerpentConsts.EnemyEggHatchingTime.TotalSeconds;
	}
	
	// Hatching behavior?
	void Update()
	{
		this.timeUntilHatched -= Time.smoothDeltaTime;
		if (this.timeUntilHatched < 0)
		{
			this.EggHatched(this);
		}
	}
}

