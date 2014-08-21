using System;
using UnityEngine;
using System.Collections.Generic;
using Serpent;

public class GameEvent
{
	public float ExecutionTime { get; set; }
	public Action Action { get; set; }
	public EventIdentifier Identifier { get; set; }
}

public class GameClock : MonoBehaviour
{
	private List<GameEvent> eventQueue = new List<GameEvent>();
	
	private float time = 0.0f;
	public float Time	
	{
		get
		{
			return this.time;
		}
	}
	
	public bool Paused { get; set; }
	
	public void Update()
	{
		if (this.Paused) { return; }
		
		float deltaTime = UnityEngine.Time.deltaTime;
		if (deltaTime > 0.1f) { deltaTime = 0.1f; }
		this.time += deltaTime;
		
		CheckEventQueue();
	}
	
	#region Event Queue
	
	public void RegisterEvent(float timeInFuture, Action action, EventIdentifier identifier = EventIdentifier.None)
	{
		GameEvent newEvent = new GameEvent();
		newEvent.ExecutionTime = this.time + timeInFuture;
		newEvent.Action = action;
		newEvent.Identifier = identifier;
		InsertInEventQueue(newEvent);
	}
	
	private void InsertInEventQueue(GameEvent newEvent)
	{
		// Insert the event by order of execution time
		if (this.eventQueue.Count == 0)
		{
			this.eventQueue.Add(newEvent);
			return;
		}
		
		for (int index = 0; index < this.eventQueue.Count; ++index)
		{
			GameEvent existingEvent = this.eventQueue[index];
			if (newEvent.ExecutionTime < existingEvent.ExecutionTime)
			{
				this.eventQueue.Insert(index, newEvent);
				return;
			}
		}
		
		// Event occurs after all existing events
		this.eventQueue.Add(newEvent);
	}
	
	public GameEvent GetEvent(EventIdentifier identifier)
	{
		foreach( GameEvent e in this.eventQueue)
		{
			if (e.Identifier == identifier)
			{
				return e;
			}
		}
		return null;
	}
	
	private void CheckEventQueue()
	{
		// A while loop is required in case there are multiple events triggered
		// at the same time.	
		while( this.eventQueue.Count > 0 )
		{
			GameEvent existingEvent = this.eventQueue[0];
			if (existingEvent.ExecutionTime < this.time)
			{
				existingEvent.Action();
				this.eventQueue.Remove(existingEvent);
				continue;
			}
			break;
		}
	}
	
	public void Reset()
	{
		this.eventQueue.Clear();
		this.time = 0.0f;
	}

	#endregion Event Queue	
}

