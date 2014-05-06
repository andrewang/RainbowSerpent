﻿using UnityEngine;
using System.Collections;
using System;
using SerpentExtensions;

public class SnakeBody : SnakeSegment 
{
	public float DistanceFromHead { get; set; }
	
	// The final segment of a snake may have an egg that it is creating.
	public Egg Egg { get; set; }
		
	public override void ResetProperties()
	{
		base.ResetProperties();
		RemoveEgg();
		this.DistanceFromHead = 0.0f;
	}
	
	public void RemoveEgg()
	{
		if (this.Egg)
		{
			this.Egg.Die();
			this.Egg = null; 
		}	
	}
	
	public void MoveEgg(SnakeBody otherSegment)
	{
		// When this method is called if this segment has an egg attached, the other segment should become the new owner of the egg.
		if (this.Egg == null) { return; }
				
		otherSegment.BeginToCreateEgg(this.Egg);
		this.Egg.FullyGrown -= this.EggFullyGrown;		
		this.Egg = null;
	}
	
	public void BeginToCreateEgg(Egg egg)
	{
		egg.SetParent(this);
		
		// Make sure egg is displayed on top of the segment
		egg.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		egg.SetSpriteDepth(5);
		
		this.Egg = egg;
		egg.FullyGrown += this.EggFullyGrown;
		egg.ContainingBody = this;
	}
	
	public void EggFullyGrown(Egg e)
	{
		// Check for null in case of conflict between timer and snake+egg death.
		if (this.Egg == null) { return; }
		
		this.Egg.SetSpriteDepth(0);
		
		this.Egg.FullyGrown -= this.EggFullyGrown;
		this.Egg = null;
		
		// this snake segment needs to be removed from its snake.		
		this.Snake.SeverAtSegment(this);
		
	}
	
	public override void OnDestroy()
	{
		base.OnDestroy();

		// If a snake dies, partway through laying an egg, we need to destroy the egg object too.
		if (this.Egg != null)
		{
			this.Egg.Die();
			this.Egg = null;
		}
	}
}
