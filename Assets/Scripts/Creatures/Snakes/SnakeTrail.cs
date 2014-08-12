using UnityEngine;
using System.Collections.Generic;
using Serpent;

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
		
		if (removeElements && i + 1 < this.positions.Count)
		{
			// The current point might not give us MaxLength of trail, so remove AFTER this element.
			this.positions = this.positions.GetRange( 0, i + 1);
		}
	}
	
	public Vector3 GetHeadPosition()
	{
		return this.positions[0].Position;
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
	
	private void UpdateUnitVector( int index )
	{
		if (index + 1 >= this.positions.Count) { return; }
		Vector3 pos1 = this.positions[index].Position;
		Vector3 pos2 = this.positions[index + 1].Position;
		Vector3 difference = pos2 - pos1;
		Vector3 unitVector = difference.normalized;
		this.positions[index].UnitVectorToPreviousPosition = unitVector;
		
	}
	
	// Loop through all segments and set their positions.
	public void PositionSegments( SnakeHead head )
	{		
		SetHeadPosition(head);
		
		int positionIndex = 0;
		SnakeBody bodySegment = head.NextSegment;
		while (bodySegment != null)
		{
			positionIndex = SetSegmentPositionAndRotation( bodySegment, positionIndex );
			bodySegment = bodySegment.NextSegment;
		}
	}
	
	private void SetHeadPosition( SnakeHead head )
	{
		// This resets the head's transform's position which may have been adjusted by sinusoidal code.
		// TODO move out to Snake
		SnakePosition posBefore = this.positions[0];
		head.transform.localPosition = posBefore.Position;
	}
	
	private int SetSegmentPositionAndRotation( SnakeBody bodySegment, int startingIndex )
	{
		// get the last position index in the trail, prior to the location of the specified body segment 
		int indexBefore = GetPositionIndexBefore( bodySegment.DistanceFromHead, startingIndex );
		if (indexBefore == -1) 
		{
			// position specified is not contained in the trail
			// extrapolate based on last position in the trail, if possible.
			// return the last position in the trail.
			SnakePosition position = this.positions[ this.positions.Count - 1 ];
			if (position.UnitVectorToPreviousPosition.magnitude > 0.0f)
			{
				indexBefore = this.positions.Count - 1;
			}
			else
			{
				// no unit vector available
				bodySegment.transform.localPosition = position.Position;
				// don't try to set rotation.				
				return 0;
			}
		}
		
		SnakePosition posBefore = this.positions[indexBefore];
		SetSegmentRotation( bodySegment, posBefore );

		Vector3 displacement = posBefore.UnitVectorToPreviousPosition * (bodySegment.DistanceFromHead - posBefore.DistanceFromHead);
		Vector3 newPos = posBefore.Position + displacement;
		bodySegment.transform.localPosition = newPos;
		
		return indexBefore;
	}
	
	private void SetSegmentRotation( SnakeSegment bodySegment, SnakePosition position )
	{
		Direction dir = SerpentConsts.GetDirectionForVector( position.UnitVectorToPreviousPosition );
		if (dir == Direction.None)
		{
			return;
		}
		
		Direction oppositeDir = SerpentConsts.OppositeDirection[ (int)dir ];
		bodySegment.CurrentDirection = oppositeDir;
	}
	
	private int GetPositionIndexBefore( float distanceFromHead, int startingIndex = 0 )
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
	
	// Return information on the closest corner behind a particular segment, for use in smoothing the path at corners
	public SnakePosition GetClosestCornerBehind( SnakeSegment segment )
	{
		float distanceFromHead = 0.0f;
		if (segment is SnakeBody)
		{
			SnakeBody body = segment as SnakeBody;			
			distanceFromHead = body.DistanceFromHead;
		}
		int indexBefore = GetPositionIndexBefore( distanceFromHead, 0 );
		return this.positions[indexBefore];
	}
	
}


