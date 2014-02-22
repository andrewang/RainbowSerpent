using UnityEngine;
using System.Collections.Generic;

public class SnakeTrail
{
	public class SnakePosition
	{
		public Vector3 Position { get; set; }
		public Vector3 UnitVectorToPreviousPosition { get; set; }
		public float DistanceFromHead { get; set; }
		
		public SnakePosition( Vector3 pos )
		{
			this.Position = pos;
		}
	}

	private List<SnakePosition> positions; 
	public List<SnakePosition> Positions
	{
		get
		{
			return this.positions;
		}
	}
	
	private const float MaxLength = SerpentConsts.CellWidth * 10.0f;
	
	public SnakeTrail ()
	{
		this.positions = new List<SnakePosition>();
		Reset();
	}
	
	public void Reset()
	{
		// Clear all
		this.positions.Clear();
		// Add a dummy head position
		SnakePosition headPosition = new SnakePosition( new Vector3( 0, 0, 0 ) );
		headPosition.DistanceFromHead = 0.0f;
		this.positions.Add( headPosition );		
	}
	
	public void UpdateHeadPosition( Vector3 pos )
	{
		positions[0].Position = pos;
		UpdateUnitVector( 0 );
		
		// Update all the distances from head values.
		Vector3 lastPos = pos;
		float totalDistance = 0.0f;
		int i = 1;
		bool removeElements = false;
		for (; i < this.positions.Count; ++i)
		{
			Vector3 nextPos = this.positions[i].Position;
			Vector3 displacement = nextPos - lastPos;
			float distance = displacement.magnitude;
			totalDistance = totalDistance + distance;
			this.positions[i].DistanceFromHead = totalDistance;
			if (totalDistance > SnakeTrail.MaxLength)
			{
				removeElements = true;
				break;
			}
			lastPos = nextPos;
		}
		
		if (removeElements)
		{
			this.positions = this.positions.GetRange( 0, i );
		}
	}
	
	public void AddPosition( Vector3 pos )
	{
		// If this position is a duplicate of the one just behind the head then don't add it.
		if (this.positions.Count > 1)
		{
			Vector3 mostRecentPosition = this.positions[1].Position;
			if (pos == mostRecentPosition)
			{
				return;
			}			
		}
		
		// Insert a new position after the head.
		SnakePosition newPosition = new SnakePosition( pos );
		
		this.positions.Insert( 1, newPosition );
		
		// Update unit vectors
		UpdateUnitVector( 1 );
		UpdateUnitVector( 0 );
	}
	
	public void UpdateUnitVector( int index )
	{
		if (index + 1 >= this.positions.Count) { return; }
		Vector3 pos1 = this.positions[index].Position;
		Vector3 pos2 = this.positions[index + 1].Position;
		Vector3 difference = pos2 - pos1;
		Vector3 unitVector = difference.normalized;
		this.positions[index].UnitVectorToPreviousPosition = unitVector;
		
	}
	
	/*
	
	public Vector3 GetSegmentPosition( float distanceFromHead )
	{
		// Interpolate (distanceFromHead) units between the appropriate positions in the list.
		int indexBefore = GetPositionIndexBefore( distanceFromHead );
		if (indexBefore == -1) 
		{
			// return the last position in the trail.
			SnakePosition position = this.positions[ this.positions.Count - 1 ];
			return position.Position;
		}
		
		SnakePosition posBefore = this.positions[indexBefore];
		Vector3 displacement = posBefore.UnitVectorToPreviousPosition * (distanceFromHead - posBefore.DistanceFromHead);
		return posBefore.Position + displacement;		
	}
	
	*/
	
	public void SetSegmentPositions( SnakeHead head )
	{
		int index = 0;
		SnakeBody bodySegment = head.NextSegment;
		while( bodySegment != null )
		{
			index = SetSegmentPosition( bodySegment, index );
			bodySegment = bodySegment.NextSegment;
		}
	}
	
	public int SetSegmentPosition( SnakeBody bodySegment, int startingIndex )
	{
		int indexBefore = GetPositionIndexBefore( bodySegment.DistanceFromHead, startingIndex );
		if (indexBefore == -1) 
		{
			// extrapolate based on last position in the trail, if possible.
			// return the last position in the trail.
			SnakePosition position = this.positions[ this.positions.Count - 1 ];
			if (position.UnitVectorToPreviousPosition.magnitude > 0.0f)
			{
				indexBefore = this.positions.Count - 1;
			}
			else
			{
				bodySegment.transform.localPosition = position.Position;
				return 0;
			}
		}
		
		SnakePosition posBefore = this.positions[indexBefore];
		Vector3 displacement = posBefore.UnitVectorToPreviousPosition * (bodySegment.DistanceFromHead - posBefore.DistanceFromHead);
		bodySegment.transform.localPosition = posBefore.Position + displacement;		
		return indexBefore;
	}
	
	
	public int GetPositionIndexBefore( float distanceFromHead, int startingIndex = 0 )
	{
		for (int i = startingIndex; i < this.positions.Count; ++i)
		{
			float d = this.positions[i].DistanceFromHead;
			if (d > distanceFromHead)
			{
				return i - 1;
			}
		}
		
		return -1;
	}
	
}


