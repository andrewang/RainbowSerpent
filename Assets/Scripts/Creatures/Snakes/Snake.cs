using System;
using UnityEngine;

public class Snake : Creature
{
	public int NumSegments
	{
		get
		{
			int numSegments = 0;
			SnakeSegment segment = this.head;
			while (segment != null)
			{
				++numSegments;
				segment = segment.NextSegment;
			}
			return numSegments;
		}
	}

	private SnakeSegment lastSegment
	{
		get
		{
			SnakeSegment segment = this.head;
			while (segment.NextSegment != null)
			{
				segment = segment.NextSegment;
			}
			return segment;
		}
	}

	private SnakeConfig config;
	private SnakeHead head;
	
	public void SetUp(SnakeConfig config, int numSegments)
	{
		this.config = config;
		for (int i = 0; i < numSegments; ++i)
		{
			AddSegment();
		}
	}

	public void AddSegment()
	{
		// This method should add a new segment at the end of the snake.  It can be in the same position
		// as the last segment and will appear when the now next-to-last segment moves away from
		// its current position.
		if (this.head == null)
		{
			GameObject newObj = (GameObject) Instantiate(this.config.HeadPrefab, new Vector3(0,0,0), Quaternion.identity);

			this.head = newObj.GetComponent<SnakeHead>();
			// how do we set head position...?
		}
		else
		{
			GameObject newObj = (GameObject) Instantiate(this.config.BodyPrefab, new Vector3(0,0,0), Quaternion.identity);
			SnakeSegment newSegment = newObj.GetComponent<SnakeSegment>();

			SnakeSegment lastSegment = this.lastSegment;
			lastSegment.NextSegment = newSegment;

			newSegment.transform.localPosition = lastSegment.transform.localPosition;
		}

	}

	public void CheckValidity()
	{
		int numSegments = this.NumSegments;
		if (numSegments == 1)		
		{
			// The body of the snake is all gone.  Time to die!
		}

	}
}

