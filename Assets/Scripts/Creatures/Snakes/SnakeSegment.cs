using UnityEngine;
using System.Collections;
using SerpentExtensions;
using Serpent;

/// <summary>
/// The snake segment is the base class for the snake body (segment) and head (segment) containing 
/// code and properties common to both.
/// </summary>
using System;

public class SnakeSegment : MonoBehaviour
{	
	#region Serialized Fields

	// Any segment will have a sprite
	[SerializeField] protected UISprite sprite;
		
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
	private Direction currentDirection;
	public Direction CurrentDirection
	{
		get
		{
			return this.currentDirection;
		}
		set
		{
			this.currentDirection = value;
			this.currentDirectionVector = SerpentConsts.DirectionVector3[ (int)value ];
			// change the rotation of the segment
			this.CurrentFacing = SerpentConsts.RotationVector3[ (int)value ];
		}
	}
	
	private Vector3 currentFacing;
	public Vector3 CurrentFacing
	{
		get
		{
			return this.currentFacing;
		}
		set
		{
			this.currentFacing = value;
			this.transform.eulerAngles = value;
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
			int alphaValue = value ? 255 : 0;
			Color c = this.sprite.color;
			c.a = alphaValue;
			this.sprite.color = c;
			
			// Set the transparency of all child 
			foreach (UISprite s in this.GetComponentsInChildren<UISprite>())
			{
				c = s.color;
				c.a = alphaValue;
				s.color = c;
			}
		}
	}
	
	#endregion Properties
	
	public virtual void ResetProperties()
	{
		// Should reset all the segment's properties.
		this.NextSegment = null;
		this.sprite.type = UISprite.Type.Filled;
	}
		
	public float DistanceSquaredTo( SnakeSegment otherSegment )
	{
		Vector3 positionDiff = this.transform.localPosition - otherSegment.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherSegment.Radius;
		float radiiSq = radii * radii;
		return distanceSq - radiiSq;
	}
	
	public float DistanceSquaredToCreature( Creature otherCreature )
	{
		Vector3 positionDiff = this.transform.localPosition - otherCreature.transform.localPosition;
		float distanceSq = positionDiff.sqrMagnitude;
		float radii = this.Radius + otherCreature.Radius;
		float radiiSq = radii * radii;
		return distanceSq - radiiSq;		
	}
	
	public virtual void OnDestroy()
	{
		if (this.sprite != null)
		{
			Destroy(this.sprite);
			this.sprite = null;
		}
	}
	
	#region Sprite
	
	public virtual void SetSpriteDepth(int depth)
	{
		this.sprite.depth = depth;
	}
	
	public int GetSpriteDepth()
	{
		return this.sprite.depth;
	}
	
	public void SetGradatedColour(Color c1, Color c2)
	{
		this.sprite.type = UISprite.Type.VertexColoured;
		this.sprite.SetVertexColours(c2, c1, c1, c2);		
	}
	
	public void SetSpriteName(string spriteName)
	{
		this.sprite.spriteName = spriteName;
	}

	#endregion Sprite	
}
