using UnityEngine;
using System.Collections;
using SerpentExtensions;

/// <summary>
/// The snake segment is the base class for the snake body (segment) and head (segment) containing 
/// code and properties common to both.
/// </summary>
using System;

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
	public Snake Snake { get; set; }
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
			// Incorporate separately-set visibility into the colour value
			float a = this.sprite.color.a;
			Color c = value;
			c.a = a;
			this.sprite.color = c;
		}
	}
	
	public bool Visible
	{
		get
		{
			Color c = this.sprite.color;
			return (c.a > 0);
		}		
		set
		{
			Color c = this.sprite.color;
			c.a = value ? 255 : 0;	
			this.sprite.color = c;
		}
	}
	
	#endregion Properties
	
	public virtual void ResetProperties()
	{
		// Should reset all the segment's properties.
		this.NextSegment = null;
	}
		
	public bool TouchesSegment( SnakeSegment otherSegment )
	{
		Vector3 positionDiff = this.transform.localPosition - otherSegment.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherSegment.Radius;
		float radiiSq = radii * radii;
		return (distanceSq <= radiiSq);
	}
	
	public bool TouchesCreature( Creature otherCreature )
	{
		Vector3 positionDiff = this.transform.localPosition - otherCreature.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherCreature.Radius;
		float radiiSq = radii * radii;
		return (distanceSq <= radiiSq);		
	}
	
	public virtual void OnDestroy()
	{
		if (this.sprite != null)
		{
			Destroy(this.sprite);
			this.sprite = null;
		}
	}
}
