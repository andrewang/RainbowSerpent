using UnityEngine;
using System.Collections;

/// <summary>
/// The snake segment is the base class for the snake body (segment) and head (segment) containing 
/// code and properties common to both.
/// </summary>

public class SnakeSegment : MonoBehaviour
{	
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
	
	public SerpentConsts.Dir NextDirection
	{
		get; set; 
	}
	
	private Vector3 currentDirectionVector;
	protected Vector3 CurrentDirectionVector
	{
		get
		{
			return this.currentDirectionVector;
		}
	}
	
	public Vector3 CurrentDestination
	{
		get; set; 
	}	

	public SnakeHead Head { get; set; }
	public SnakeBody NextSegment { get; set; }
	public Snake Owner { get; set; }
	
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
	
	// Any segment will have a sprite
	[SerializeField] private UISprite sprite;

	virtual public void UpdatePosition(SerpentConsts.Dir parentDirection, float distance)
	{
	}
		
	protected void UpdateNextSegmentPosition(SerpentConsts.Dir parentDirection, float distance)
	{
		if (this.NextSegment == null ) { return; }
		this.NextSegment.UpdatePosition( parentDirection, distance );
	}
}
