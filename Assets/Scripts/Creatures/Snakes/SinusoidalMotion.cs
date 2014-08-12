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
	
	/*
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
		float percent = distToNextCellCentre / fullDist;
		if (percent >= 1.0f)
		{
			// wtf
			percent = percent - Mathf.Floor(percent);
		}
		return percent;
	}
	*/
	
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


