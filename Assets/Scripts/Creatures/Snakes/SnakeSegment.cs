using UnityEngine;
using System.Collections;

/// <summary>
/// The snake segment is the base class for the snake body (segment) and head (segment) containing 
/// code and properties common to both.
/// </summary>

public class SnakeSegment : MonoBehaviour
{
	// The direction the segment is currently going.
	public int CurrDirection { get; set; }
	// The direction the segment should change to after reaching the centre of the next tile.
	// When a snake changes direction, the head changes direction first, and sets the next
	// segment's NextDirection.
	public int NextDirection { get; set; }

	public SnakeSegment Head { get; set; }
	public SnakeSegment NextSegment { get; set; }
	public Snake Owner { get; set; }
	
	// Any segment will have a sprite
	public UISprite sprite;

	// Use this for initialization
	void Start () {
	
	}

}
