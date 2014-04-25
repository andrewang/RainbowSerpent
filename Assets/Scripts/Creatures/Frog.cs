using UnityEngine;
using System;
using System.Collections.Generic;

public class Frog : MobileCreature
{
	private DateTime moveTime;
	
	private GameSceneController sceneController;
	
	private void Start ()
	{
		UpdateTimer();
	}
	
	public void SetUp( GameSceneController sceneController )
	{
		this.sceneController = sceneController;
	}
	
	private void Update ()
	{
		if (DateTime.Now > this.moveTime)
		{
			Move();
		}		
	}
	
	private void UpdateTimer()
	{
		this.moveTime = DateTime.Now + new TimeSpan(0, 0, 5);
	}
	
	private void Move ()
	{
		// If there is a snake nearby, move to avoid it.  Potentially move off the map.
		
		
		// If there is an egg, move to eat it.
		/*
		Egg egg = GetNearestEgg();
		if (egg == null)
		{
		}
		*/
		
		// Random move?
		List<SerpentConsts.Dir> validDirections = this.MazeController.GetValidDirections(this.transform.localPosition, false);
		if (validDirections.Count == 0) { return; }

		int roll = UnityEngine.Random.Range(0, validDirections.Count);		
		this.CurrentDirection = validDirections[roll];
		
		Vector3 dest = this.MazeController.GetNextCellCentre(this.transform.localPosition, this.CurrentDirection);
		this.CurrentDestination = dest;			
	}
	
	private Snake GetNearestSnake()
	{		
		List<Creature> snakes = this.sceneController.GetSnakes();
		return GetNearestCreature( snakes ) as Snake;
	}
	
	private Egg GetNearestEgg()
	{
		List<Creature> eggs = this.sceneController.GetEggs();		
		return GetNearestCreature( eggs ) as Egg;
	}
	

	
}