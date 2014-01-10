using UnityEngine;
using System.Collections;

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
	
	// The final segment of a snake may have an egg that it is creating.
	private Egg egg;
	private event Action<SnakeSegment,Egg> eggFullyGrown;	
	private event Action<Egg> eggDestroyed;
	
	#endregion Properties
	
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
	
	public void BeginToCreateEgg(Egg egg, Action<SnakeSegment,Egg> eggFullyGrown, Action<Egg> eggDestroyed)
	{
		// Attach the egg to this segment
		egg.transform.parent = this.transform;
		// Make sure egg is displayed on top of the segment
		egg.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
		egg.SetSpriteDepth(5);
		
		egg.FullyGrown += this.EggFullyGrown;
		this.egg = egg;
		
		this.eggFullyGrown += eggFullyGrown;
		this.eggDestroyed += eggDestroyed;
	}
	
	private void EggFullyGrown()
	{
		egg.SetSpriteDepth(0);
		this.eggFullyGrown(this, this.egg);
		this.egg = null;
		
		// this snake segment needs to be removed from its snake.		
		this.Snake.SeverAtSegment(this);
		
	}
	
	public void OnDestroy()
	{
		if (this.sprite != null)
		{
			Destroy(this.sprite);
			this.sprite = null;
		}
		if (this.egg != null)
		{
			this.eggDestroyed(this.egg);
			Destroy(this.egg.gameObject);
			this.egg = null;
		}
	}
}
