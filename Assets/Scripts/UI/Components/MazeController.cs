using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeController : MonoBehaviour
{
	[SerializeField] private Maze maze = null;
	[SerializeField] private GameObject wallSpritePrefab = null;
	[SerializeField] private ScreenShotTaker screenShotTaker = null; 
	[SerializeField] private UIPanel panel = null;
	private Color wallColour; 
	private bool wallSpritesCollated = false;

	/// <summary>
	/// The centre position of the lower leftmost cell in the maze
	/// </summary>
	private Vector3 lowerLeftCellCentre = new Vector3(0,0,0);

	#region Verify Serialize Fields

	public void Start()
	{
		VerifySerializeFields();
	}
	
	public void VerifySerializeFields()
	{
		if (this.maze == null) { Debug.LogError("GameSceneController: maze is null"); }
	}
	
	#endregion Verify Serialize Fields
	
	private void Update()
	{
		if (Input.GetKeyDown("k") && this.wallSpritesCollated == false)
		{
			CollateWallSprites();
			this.wallSpritesCollated = true;
		}
	}
	
	/// <summary>
	/// Sets up the maze controller
	/// </summary>
	/// <param name="mazeTextAsset">Maze text asset.</param>
	public void SetUp(TextAsset mazeTextAsset, Color wallColour)
	{
		this.wallColour = wallColour;
		this.maze.SetUp(mazeTextAsset);
		DetermineSpriteScale();		
		CreateMazeSprites();
	}
	
	private void DetermineSpriteScale()
	{
		Vector4 spriteBounds = this.panel.clipRange;
		// clip range is ZW.
		float maxWidth = spriteBounds.z;
		float maxHeight = spriteBounds.w;
	
		// add a bit to the dimensions so that the outside walls don't get clipped
		float q = maxWidth / ((this.maze.Width + 0.2f) * SerpentConsts.CellWidth);
		float x = maxHeight / ((this.maze.Height + 0.2f) * SerpentConsts.CellHeight);
		float scale = Mathf.Min(q, x);
		this.transform.localScale = new Vector3(scale, scale, 0.0f);
	}

	/// <summary>
	/// Creates the maze sprites.  These will be centred on the origin of the maze controller.
	/// </summary>
	private void CreateMazeSprites()
	{
		int width = this.maze.Width;
		int height = this.maze.Height;
		if (width == 0 || height == 0) { return; }

		// Determine the lower left corner of the map.
		this.lowerLeftCellCentre.x = -1.0f * ((width - 1) * SerpentConsts.CellWidth) * 0.5f;
		this.lowerLeftCellCentre.y = -1.0f * ((height - 1) * SerpentConsts.CellHeight) * 0.5f;

		CreateHorizontalWallSprites();
		CreateVerticalWallSprites();
		
		CreateHorizontalWallSprite(0, width - 1, 0, (int) SerpentConsts.Dir.S);		
		CreateHorizontalWallSprite(0, width - 1, height - 1, (int) SerpentConsts.Dir.N);
		CreateVerticalWallSprite(0, height - 1, 0, (int) SerpentConsts.Dir.W);		
		CreateVerticalWallSprite(0, height - 1, width - 1, (int) SerpentConsts.Dir.E);
	}
	
	private void CreateHorizontalWallSprites()
	{
		int height = this.maze.Height;
		for (int y = 0; y < height; ++y)
		{
			CreateHorizontalWallSpritesForRow( y, (int) SerpentConsts.Dir.S );
		}		
		CreateHorizontalWallSpritesForRow( height - 1, (int) SerpentConsts.Dir.N );	
	}
	
	private void CreateHorizontalWallSpritesForRow( int y, int intSide )
	{
		int start = -1;
		for (int x = 0; x < this.maze.Width; ++x)
		{
			MazeCell cell = this.maze.Cells[x,y];
			
			if (cell.Walls[intSide] != null)
			{
				if (start == -1)
				{
					// start of a wall
					start = x;
				}
			}
			else if (start >= 0)
			{
				// end of a wall
				CreateHorizontalWallSprite( start, x - 1, y, intSide);
				start = -1;
			}
		}	
	}
	
	private void CreateHorizontalWallSprite( int x0, int x1, int y, int intSide )
	{			
		GameObject newWall = (GameObject) Instantiate(this.wallSpritePrefab, new Vector3(0,0,0), Quaternion.identity);
		newWall.transform.parent = this.transform;
		
		Vector3 pos = GetCellSideCentre( (float)(x1 + x0) * 0.5f, y, intSide);
		newWall.transform.localPosition = pos;
		newWall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
				
		UISprite newWallSprite = newWall.GetComponent<UISprite>();
		if (newWallSprite == null) 
		{ 
			return; 
		}
		newWallSprite.color = this.wallColour;
		
		// Horizontal
		newWallSprite.width = SerpentConsts.CellWidth * (x1 - x0 + 1);
	}
	
	private void CreateVerticalWallSprites()
	{
		int width = this.maze.Width;
		for (int x = 0; x < width; ++x)
		{
			CreateVerticalWallSpritesForColumn( x, (int) SerpentConsts.Dir.E );
		}		
		CreateVerticalWallSpritesForColumn( width - 1, (int) SerpentConsts.Dir.W );	
	}
	
	private void CreateVerticalWallSpritesForColumn( int x, int intSide )
	{
		int start = -1;
		for (int y = 0; y < this.maze.Height; ++y)
		{
			MazeCell cell = this.maze.Cells[x,y];
			
			if (cell.Walls[intSide] != null)
			{
				if (start == -1)
				{
					// start of a wall
					start = y;
				}
			}
			else if (start >= 0)
			{
				// end of a wall
				CreateVerticalWallSprite( start, y - 1, x, intSide);
				start = -1;
			}
		}	
	}
	
	
	private void CreateVerticalWallSprite( int y0, int y1, int x, int intSide )
	{		
		GameObject newWall = (GameObject) Instantiate(this.wallSpritePrefab, new Vector3(0,0,0), Quaternion.identity);
		newWall.transform.parent = this.transform;
		
		Vector3 pos = GetCellSideCentre( x, (float)(y1 + y0) * 0.5f, intSide);
		newWall.transform.localPosition = pos;
		newWall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		UISprite newWallSprite = newWall.GetComponent<UISprite>();
		if (newWallSprite == null) 
		{ 
			return; 
		}
		newWallSprite.color = this.wallColour;
		
		// Horizontal
		
		newWallSprite.width = SerpentConsts.CellHeight * (y1 - y0 + 1);
		// Rotate the sprite to be vertical
		newWallSprite.transform.Rotate(Vector3.forward * 90.0f);
	}	
	
	/// <summary>
	/// Creates a wall sprite.
	/// </summary>
	/// <param name="cell">Cell.</param>
	/// <param name="side">The side of the cell on which to make the sprite.</param>
	private void CreateWallSprite(MazeCell cell, SerpentConsts.Dir side)
	{
		int intSide = (int)side;
		if (cell.Walls[intSide] == null) 
		{
			return; 
		}

		// Create and locate the sprite.
		GameObject newWall = (GameObject) Instantiate(this.wallSpritePrefab, new Vector3(0,0,0), Quaternion.identity);
		newWall.transform.parent = this.transform;

		Vector3 pos = GetCellSideCentre(cell.X, cell.Y, intSide);
		newWall.transform.localPosition = pos;
		newWall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		//Debug.Log("Creating wall sprite at  " + pos.x + "," + pos.y + " for cell " + cell.X + "," + cell.Y + " side " + SerpentConsts.DirectionChar[ intSide ]);

		UISprite newWallSprite = newWall.GetComponent<UISprite>();
		if (newWallSprite == null) 
		{ 
			return; 
		}
		newWallSprite.color = this.wallColour;

		// Note, we're assuming here that the wall sprite is horizontal, not vertical.  So if the wall
		// is actually vertical, we have to set the rotation
		if (side == SerpentConsts.Dir.N || side == SerpentConsts.Dir.S)
		{
			// Horizontal
			newWallSprite.width = SerpentConsts.CellWidth;
		}
		else
		{
			// Vertical
			newWallSprite.width = SerpentConsts.CellHeight;
			// Rotate the sprite to be vertical
			newWallSprite.transform.Rotate(Vector3.forward * 90.0f);
		}
	}
	
	// This method should create a single PNG image of all the wall sprites existing at this point.  The game can then
	// continue to use this sprite instead of all the individual sprite objects, of which there many.
	// To do this, we need access to the camera and to create a RenderTexture
	private void CollateWallSprites()
	{
	/*
		// As a test, start by writing out a screenshot
		int resWidth = Screen.width;
		int resHeight = Screen.height;
		RenderTexture rt = new RenderTexture(resWidth, resHeight, 32);
		sceneCamera.targetTexture = rt;
		Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
		sceneCamera.Render();
		RenderTexture.active = rt;
		screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
		sceneCamera.targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy(rt);
		byte[] bytes = screenShot.EncodeToPNG();
		string filename = "maze.png";
		System.IO.File.WriteAllBytes(filename, bytes);
		*/
		this.screenShotTaker.TakeScreenShot(0);		
	}


	/// <summary>
	/// Gets the centre position of a cell
	/// </summary>
	/// <returns>The cell centre.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Vector3 GetCellCentre(float x, float y)
	{
		Vector3 pos = this.lowerLeftCellCentre;
		pos.x += x * SerpentConsts.CellWidth;
		pos.y += y * SerpentConsts.CellHeight;
		return pos;
	}
	
	/// <summary>
	/// Gets the maze cell containing the provided position.
	/// </summary>
	/// <returns>The cell for position.</returns>
	/// <param name="position">Position.</param>
	public MazeCell GetCellForPosition(Vector3 position)
	{
		Vector3 displacement = position - this.lowerLeftCellCentre;		
		int x = (int) (displacement.x / SerpentConsts.CellWidth + 0.5f);
		int y = (int) (displacement.y / SerpentConsts.CellHeight + 0.5f);
		if (x >= this.maze.Width || y >= this.maze.Height) { return null; }
		return this.maze.Cells[x,y];
	}
	
	public Vector3 GetNextCellCentre(Vector3 position, SerpentConsts.Dir direction)
	{
		// Convert the Vector3 position into a floating point position measured
		// in cells.  Then adjust the position by a step in the direction specified,
		// and then round off the position to determine which cell that is.
		
		Vector3 pos = position - this.lowerLeftCellCentre;
		Vector3 cellPos = pos;
		cellPos.x /= (float)SerpentConsts.CellWidth;
		cellPos.y /= (float)SerpentConsts.CellHeight;
		cellPos += SerpentConsts.DirectionVector3[(int) direction];
		
		// now round to nearest position		
		IntVector2 intCellPos = new IntVector2(0,0);		
		intCellPos.x = (int) (cellPos.x + 0.5f);
		intCellPos.y = (int) (cellPos.y + 0.5f);
		
		// Get the centre position of this cell, and return it.		
		Vector3 newPos = GetCellCentre(intCellPos.x, intCellPos.y);
		return newPos;
	}	
	
	/// <summary>
	/// Return whether motion is blocked from a given position, in a given direction.
	/// </summary>
	/// <returns><c>true</c>, if motion was blocked, <c>false</c> otherwise.</returns>
	/// <param name="position">Position.</param>
	/// <param name="direction">Direction.</param>
	public bool IsMotionBlocked(Vector3 position, SerpentConsts.Dir direction)
	{
		MazeCell cell = GetCellForPosition(position);
		if (cell == null) { return true; }
		
		return cell.IsMotionBlocked(direction);
	}
	
	public List<SerpentConsts.Dir> GetValidDirections(Vector3 position, bool allowOffscreen)
	{
		List<SerpentConsts.Dir> availableDirections = new List<SerpentConsts.Dir>();
		if (allowOffscreen)
		{
			for (int i = 0; i < SerpentConsts.NumDirections; ++i)
			{
				SerpentConsts.Dir dir = (SerpentConsts.Dir) i;
				availableDirections.Add(dir);
			}		
			return availableDirections;
		}
		
		MazeCell cell = GetCellForPosition(position);
		
		if (cell.X > 0) 
		{
			availableDirections.Add( SerpentConsts.Dir.W ); 
		}
		if (cell.X < this.maze.Width)
		{
			availableDirections.Add( SerpentConsts.Dir.E ); 
		}
		if (cell.Y > 0)
		{
			availableDirections.Add( SerpentConsts.Dir.S );
		}
		if (cell.Y < this.maze.Height)
		{
			availableDirections.Add( SerpentConsts.Dir.N );
		}
		
		return availableDirections;
	}
	/// <summary>
	/// Gets the centre position for a cell side, to place a wall there
	/// </summary>
	/// <returns>The cell side's centre.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="side">Side.</param>
	private Vector3 GetCellSideCentre(float x, float y, int intSide)
	{
		float fx = x + SerpentConsts.DirectionVector3[intSide].x * 0.5f;
		float fy = y + SerpentConsts.DirectionVector3[intSide].y * 0.5f;

		Vector3 pos = GetCellCentre(fx, fy);
		return pos;
	}
}

