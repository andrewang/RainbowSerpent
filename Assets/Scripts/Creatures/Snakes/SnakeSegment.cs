﻿using UnityEngine;
using System.Collections;

/// <summary>
/// The snake segment is the base class for the snake body (segment) and head (segment) containing 
/// code and properties common to both.
/// </summary>

public class SnakeSegment : MonoBehaviour
{	
	#region Serialized Fields

	// Any segment will have a sprite
	[SerializeField] private UISprite sprite;
		
	#endregion Serialized Fields
	
	#region Properties
	
	private Vector3 currentDirectionVector;
	protected Vector3 CurrentDirectionVector
	{
		get
		{
			return this.currentDirectionVector;
		}
	}
	
	/// <summary>
	/// The current direction.  Creatures can only change direction at the centre of tiles
	/// </summary>
	private SerpentConsts.Dir currentDirection;
	public SerpentConsts.Dir CurrentDirection
	{
		get
		{
			return currentDirection;
		}
		set
		{
			this.currentDirection = value;
			this.currentDirectionVector = SerpentConsts.DirectionVector3[ (int)value ];
			// change the rotation of the segment
			this.transform.eulerAngles = SerpentConsts.RotationVector3[ (int)value ];
		}
	}
	
	public Vector3 CurrentDestination { get; set; }	
	public SnakeHead Head { get; set; }
	public SnakeBody NextSegment { get; set; }
	
	public float Width 
	{
		get
		{
			return this.sprite.width;
		}
	}
	
	public float Height 
	{
		get
		{
			return this.sprite.height;
		}
	}
	
	private float Radius
	{
		get
		{
			// Average width and height and then halve again in order to get a radius value.
			return (this.sprite.height + this.sprite.width) * 0.25f;
		}
	}
	
	public Color Colour	
	{
		get
		{
			return this.sprite.color;
		}
		set
		{
			this.sprite.color = value;
		}
	}
	
	public bool TouchesSegment( SnakeSegment otherSegment )
	{
		Vector3 positionDiff = this.transform.localPosition - otherSegment.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherSegment.Radius;
		float radiiSq = radii * radii;
		return (distanceSq <= radiiSq);
	}
	
	#endregion Properties
	
	public void OnDestroy()
	{
		if (this.sprite != null)
		{
			Destroy(this.sprite);
			this.sprite = null;
		}
	}
}
