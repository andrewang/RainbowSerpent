using System;
using UnityEngine;
using Serpent;

public class SinusoidalMotion
{
	private SnakeTrail trail;
		
	public SinusoidalMotion(SnakeTrail trail)
	{
		// to handle curves, we want access to all the trail data.
		this.trail = trail;
	}
	
	// Loop through all segments and set their positions.
	public void PositionSegments( Snake snake )
	{		
		SnakeHead head = snake.Head;
		float speed = snake.Speed;
		
		SetSegmentSinusoidalPosition(head, head.transform.localPosition, speed);
		SnakeSegment bodySegment = head.NextSegment;
		while (bodySegment != null)
		{
			SetSegmentSinusoidalPosition(bodySegment, bodySegment.transform.localPosition, speed);	
			bodySegment = bodySegment.NextSegment;			
		}
	}
	
	private void SetSegmentSinusoidalPosition(SnakeSegment segment, Vector3 basePosition, float snakeSpeed)
	{
		SnakeTrail.SnakePosition lastCorner = this.trail.GetClosestCornerBehind(segment);
		
		float distanceInCellsSinceCorner = GetDistanceInCellsSinceLastCorner(segment, lastCorner.Position);
		float sinInterpolationPercent = GetInterpolationPercent(distanceInCellsSinceCorner);
		
		if (distanceInCellsSinceCorner < 0.5f)
		{			
			// Calculate where based on sin we WANT to end up in half a tile.
			Vector3 sinPosition = GetSinusoidalPosition(segment, sinInterpolationPercent, basePosition);			
			segment.transform.localPosition = sinPosition;
			
			float angleInterpolation = Mathf.Sqrt(distanceInCellsSinceCorner / 0.5f);
			
			Vector3 angles = InterpolateFacing(segment, lastCorner, angleInterpolation);
			segment.transform.eulerAngles = angles;				
		}
		else
		{
			Vector3 sinPosition = GetSinusoidalPosition(segment, sinInterpolationPercent, basePosition);
			Vector3 sinAngles = GetSinusoidalAngle(segment, sinInterpolationPercent);
			segment.transform.localPosition = sinPosition;
			segment.transform.eulerAngles = sinAngles;
		}
	}
	
	private float GetInterpolationPercent(float distanceInCells)
	{	
		// Go through the whole animation over the distance of 2 cells.
		float numReptitionsOfAnimation = distanceInCells / 2.0f;
		float percent = numReptitionsOfAnimation - Mathf.Floor(numReptitionsOfAnimation);
		return percent;
	}
	
	private float GetDistanceInCellsSinceLastCorner(SnakeSegment segment, Vector3 lastCorner)
	{
		Vector3 fromLastTurn = segment.transform.localPosition - lastCorner;
		float cellLength;
		if (segment.CurrentDirection == Direction.N || segment.CurrentDirection == Direction.S)	
		{
			cellLength = SerpentConsts.CellHeight;
		}
		else // W/E
		{
			cellLength = SerpentConsts.CellWidth;			
		}
		float distanceInCells = fromLastTurn.magnitude / cellLength;
		return distanceInCells;
	}
	
	private float GetLengthOfSinPeriod(SnakeSegment segment)
	{
		if (segment.CurrentDirection == Direction.N || segment.CurrentDirection == Direction.S)	
		{
			return 2.0f * SerpentConsts.CellHeight;
		}
		else // W/E
		{
			return 2.0f * SerpentConsts.CellWidth;			
		}
	}
	
	private float GetSidewaysDisplacement(float interpolationPercent)
	{		
		// The sideways displacement is governed by the sin function since we want a trig function
		// which has the property of returning 0 at time=0 and at end time.		
		float finalValue = Mathf.Sin(interpolationPercent * 2 * Mathf.PI) * SerpentConsts.SinusoidalAmplitude;
		return finalValue;
	}
	
	private Vector3 GetSinusoidalPosition(SnakeSegment segment, float interpolationPercent, Vector3 basePosition)
	{		
		float sidewaysDisplacement = GetSidewaysDisplacement(interpolationPercent);
		
		Direction currentDirection = segment.CurrentDirection;
		int intRightAngleDirection = ((int)currentDirection + 1) % (int)Direction.Count;
		Vector3 rightAngleUnitVector = SerpentConsts.DirectionVector3[ intRightAngleDirection ];
		basePosition = basePosition + rightAngleUnitVector * sidewaysDisplacement;
		return basePosition;
	}
	
	private Vector3 GetSinusoidalAngle(SnakeSegment segment, float interpolationPercent)
	{		
		// Remember, the snake will travel sideways a distance equal to 2x sideways displacement while it travels
		// a distance of 2 x cells
		float instantLength = 0.001f;
		
		// Determine the length of the distance that will be travelled forward during this instant
		float sinPeriod = GetLengthOfSinPeriod(segment);		
		float futureForwardDisplacement = sinPeriod * instantLength;
		
		// Determine what the sideways displacement will be in that future instant
		float futureInstantPercent = interpolationPercent + instantLength;		
		float currentSidewaysDisplacement = GetSidewaysDisplacement(interpolationPercent);
		float futureSidewaysDisplacement = GetSidewaysDisplacement(futureInstantPercent);
		
		float sidewaysDisplacementDelta = futureSidewaysDisplacement - currentSidewaysDisplacement;
		
		// remove but remember the sign.  We have to actually do a subtraction normally rather than addition for this to work right, so...
		float signOfAjustment = -1.0f;
		if (sidewaysDisplacementDelta < 0.0f)
		{
			signOfAjustment = 1.0f;
			sidewaysDisplacementDelta *= -1.0f;
		}
		
		float hypoteneuseLength = Mathf.Sqrt((futureForwardDisplacement * futureForwardDisplacement) + (sidewaysDisplacementDelta * sidewaysDisplacementDelta));
		float sinInteriorAngle = futureForwardDisplacement/hypoteneuseLength;
		float interiorAngle = Mathf.Asin(sinInteriorAngle);
		float interiorAngleInDegrees = interiorAngle * Mathf.Rad2Deg;
		float exteriorAngleInDegrees = 90.0f - interiorAngleInDegrees;
		
		float angleAdjustment = exteriorAngleInDegrees * signOfAjustment;
		Vector3 facing = segment.CurrentFacing;
		facing.z += angleAdjustment;
		return facing;
	}
	
	private Vector3 InterpolateFacing(SnakeSegment segment, SnakeTrail.SnakePosition lastCorner, float interpolation)
	{
		Vector3 firstDirectionVector = lastCorner.UnitVectorToPreviousPosition * -1.0f;
		Direction firstDirection = SerpentConsts.GetDirectionForVector(firstDirectionVector);
		Direction secondDirection = segment.CurrentDirection;
		
		Vector3 firstEulerAngles = SerpentConsts.RotationVector3[(int)firstDirection];
		Vector3 secondEulerAngles = SerpentConsts.RotationVector3[(int)secondDirection];
		// Compare the sign of the angles and make sure they are both positive or both negative to account for the circle.
		if (firstEulerAngles.z * secondEulerAngles.z < 0.0f)
		{
			// sign problem to do with west (90) and south (-180)
			if (firstEulerAngles.z < -90.0f)
			{
				firstEulerAngles *= -1.0f;
			}
			else
			{
				secondEulerAngles *= -1.0f;
			}
		}
		Vector3 currentEulerAngles = firstEulerAngles * (1.0f - interpolation) + secondEulerAngles * interpolation;
		return currentEulerAngles;		
	}
	
}


