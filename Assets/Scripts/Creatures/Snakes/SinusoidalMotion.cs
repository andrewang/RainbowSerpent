using System;
using UnityEngine;
using Serpent;

public class SinusoidalMotion
{
	private SnakeTrail trail;
	private MazeController mazeController;
	
	// These rotations have to be filled in based on the maximum of sideways displacement and the speed of the snake.
	private Vector3[] sinusoidalRotation = new Vector3[]
	{
		new Vector3( 0, 0, -1.0f ),
		new Vector3( 0, 0, 0.0f ),
		new Vector3( 0, 0, 1.0f ),
		new Vector3( 0, 0, 0.0f ),
	};
	
	public SinusoidalMotion(SnakeTrail trail, MazeController mazeController)
	{
		// to handle curves, we want access to all the trail data.
		this.trail = trail;
		this.mazeController = mazeController;
	}
	
	// Loop through all segments and set their positions.
	public void PositionSegments( Snake snake, float sinusoidalAnimationFrame )
	{		
		SnakeHead head = snake.Head;
		float speed = snake.Speed;
		
		SetSegmentSinusoidalPosition(head, head.transform.localPosition, speed, sinusoidalAnimationFrame);
		SnakeSegment bodySegment = head.NextSegment;
		while (bodySegment != null)
		{
			// each segment is one frame behind the one in front.
			sinusoidalAnimationFrame -= 1.0f;
			if (sinusoidalAnimationFrame < 0.0f)
			{
				sinusoidalAnimationFrame += (float) SerpentConsts.SinusoidalPosition.Length;
			}
			
			SetSegmentSinusoidalPosition(bodySegment, bodySegment.transform.localPosition, speed, sinusoidalAnimationFrame);	
			bodySegment = bodySegment.NextSegment;			
		}
	}	
	
	public void UpdateAngles( Snake snake )
	{
		// Determine what the angle array should contain based on the speed and max sideways displacement of the snake.
		
		// To do this, we do a bit of trig.  First we want the angle in a triangle with
		// height Snake.speed
		// width twice amplitude 
		// (because snake motion goes from one side of the central line to the other)
 		// Then we want to calculate 90 degrees minus that angle to get the maximum amount of the snake segment rotation.
		float amplitude = SerpentConsts.SinusoidalAmplitude;
		float twiceAmplitude = amplitude * 2.0f;
		float speed = snake.Speed;
		float hypoteneuseLength = Mathf.Sqrt((speed * speed) + (twiceAmplitude * twiceAmplitude));
		float sinInteriorAngle = speed/hypoteneuseLength;
		float interiorAngle = Mathf.Asin(sinInteriorAngle);
		float interiorAngleInDegrees = interiorAngle * Mathf.Rad2Deg;
		float exteriorAngleInDegrees = 90.0f - interiorAngleInDegrees;
		this.sinusoidalRotation[0].z = -exteriorAngleInDegrees;
		this.sinusoidalRotation[2].z = exteriorAngleInDegrees;
	}
	
	private void SetSegmentSinusoidalPosition(SnakeSegment segment, Vector3 basePosition, float snakeSpeed, float sinusoidalAnimationFrame)
	{
		// Need to take corner into account!  How close is this segment to a corner?  If it is less than 1/2 tile from the
		// last corner, the calculations are totally different.
		//SnakeTrail.SnakePosition lastCorner = this.trail.GetClosestCornerBehind(segment);
		//Vector3 toLastCorner = lastCorner.Position - segment.transform.localPosition;
		//float distanceToLastCorner = toLastCorner.magnitude;
		
		// shift to not use sinusoidalAnimationFrame but map-based calculation.
		//float interpolationPercent = GetInterpolationPercent(sinusoidalAnimationFrame);
		float interpolationPercent = GetInterpolationPercent(segment);
		
		float sidewaysDisplacement = GetSidewaysDisplacement(interpolationPercent);
		segment.transform.localPosition = GetSinusoidalPosition(segment, sidewaysDisplacement, basePosition);		
		segment.transform.eulerAngles = GetSinusoidalAngle(segment, interpolationPercent);
	}
	
	private float GetInterpolationPercent(float sinusoidalAnimationFrame)
	{
		return sinusoidalAnimationFrame / this.sinusoidalRotation.Length;
	}

	private float GetInterpolationPercent(SnakeSegment segment)
	{
		Vector3 nextCellCentre = this.mazeController.GetNextCellCentre(segment.transform.localPosition, segment.CurrentDirection);
		Vector3 toNextCellCentre = nextCellCentre - segment.transform.localPosition;
		float distToNextCellCentre = toNextCellCentre.magnitude;
		float fullDist;
		if (segment.CurrentDirection == Direction.N || segment.CurrentDirection == Direction.S)	
		{
			fullDist = SerpentConsts.CellHeight;
		}
		else // W/E
		{
			fullDist = SerpentConsts.CellWidth;			
		}
		// If we want the snakes to go through the sin function at an increased rate, this is the place to change it.
		//fullDist *= 2.0f; 
		float percent = distToNextCellCentre / fullDist;
		if (percent >= 1.0f)
		{
			// wtf
			percent = percent - Mathf.Floor(percent);
		}
		return percent;
	}
	
	private float GetSidewaysDisplacement(float interpolationPercent)
	{		
		// The sideways displacement is governed by the sin function since we want a trig function
		// which has the property of returning 0 at time=0 and at end time.		
		float finalValue = Mathf.Sin (interpolationPercent * 2 * Mathf.PI);
		return finalValue;
	}
	
	private Vector3 GetSinusoidalPosition(SnakeSegment segment, float sidewaysDisplacement, Vector3 basePosition)
	{		
		Direction currentDirection = segment.CurrentDirection;
		int intRightAngleDirection = ((int)currentDirection + 1) % (int)Direction.Count;
		Vector3 rightAngleUnitVector = SerpentConsts.DirectionVector3[ intRightAngleDirection ];
		basePosition = basePosition + rightAngleUnitVector * sidewaysDisplacement;
		return basePosition;
	}
	
	private Vector3 GetSinusoidalAngle(SnakeSegment segment, float interpolationPercent)
	{
		if (interpolationPercent == 1) { interpolationPercent = 0.0f; }
		float sinusoidalAnimationFrame = this.sinusoidalRotation.Length * interpolationPercent;
	
		float wholePart = Mathf.Floor(sinusoidalAnimationFrame);
		float fractionalPart = sinusoidalAnimationFrame - wholePart;
		int index = (int)wholePart;
		Vector3 firstValue = this.sinusoidalRotation[index];
		int secondIndex = (index + 1) % this.sinusoidalRotation.Length;
		Vector3 secondValue = this.sinusoidalRotation[secondIndex];
		
		Vector3 addition = secondValue - firstValue;
		addition *= fractionalPart;
		
		Vector3 adjustment = firstValue + (secondValue - firstValue) * fractionalPart;
		Vector3 finalValue = segment.CurrentFacing + adjustment;
		return finalValue;
	}
	
}


