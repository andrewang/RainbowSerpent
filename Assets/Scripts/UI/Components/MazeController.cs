using System;
using UnityEngine;
using System.Collections;

public class MazeController : MonoBehaviour
{
	[SerializeField] private Maze maze = null;
	[SerializeField] private GameObject wallSpritePrefab = null; 

	/// <summary>
	/// The centre position of the lower leftmost cell in the maze
	/// </summary>
	private Vector3 lowerLeftCorner = new Vector3(0,0,0);

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
	
	/// <summary>
	/// Sets up the maze controller
	/// </summary>
	/// <param name="mazeTextAsset">Maze text asset.</param>
	public void SetUp(TextAsset mazeTextAsset)
	{
		this.maze.SetUp(mazeTextAsset);
		CreateMazeSprites();
	}
	
	public IEnumerator Test()
	{
		yield return new WaitForSeconds(5.0f);
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
		this.lowerLeftCorner.x = -1 * (width * SerpentConsts.CellWidth) / 2;
		this.lowerLeftCorner.y = -1 * (height * SerpentConsts.CellHeight) / 2;

		// most of the walls can be rendered by checking the south and west edges
		int y;
		int x;
		for (y = 0; y < height; ++y)
		{
			for (x = 0; x < width; ++x)
			{
				MazeCell cell = this.maze.Cells[x,y];
				CreateWallSprite(cell, SerpentConsts.Dir.S);
				CreateWallSprite(cell, SerpentConsts.Dir.W);
				if (x == width - 1)
				{
					CreateWallSprite(cell, SerpentConsts.Dir.E);
				}
				if (y == height - 1)
				{
					CreateWallSprite(cell, SerpentConsts.Dir.N);
				}
			}
		}

	}

	private void CreateWallSprite(MazeCell cell, SerpentConsts.Dir side)
	{
		int intSide = (int)side;
		if (cell.Walls[intSide] == null) { return; }

		// Create and locate the sprite.
		GameObject newWall = (GameObject) Instantiate(this.wallSpritePrefab, new Vector3(0,0,0), Quaternion.identity);
		newWall.transform.parent = this.transform;

		Vector3 pos = GetCellSideCentre(cell.X, cell.Y, intSide);
		newWall.transform.localPosition = pos;
		newWall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		
		Debug.Log("Creating wall sprite at  " + pos.x + "," + pos.y + " for cell " + cell.X + "," + cell.Y + " side " + intSide);

		UISprite newWallSprite = newWall.GetComponent<UISprite>();
		if (newWallSprite == null) { return; }

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
			newWallSprite.transform.Rotate(Vector3.forward * 90.0f);
		}

	}


	/// <summary>
	/// Gets the centre position of a cell
	/// </summary>
	/// <returns>The cell centre.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Vector3 GetCellCentre(float x, float y)
	{
		Vector3 pos = this.lowerLeftCorner;
		pos.x += x * SerpentConsts.CellWidth;
		pos.y += y * SerpentConsts.CellHeight;
		return pos;
	}
	
	private MazeCell GetCellForPosition(Vector3 position)
	{
		Vector3 displacement = position - this.lowerLeftCorner;
		int x = (int) (displacement.x / SerpentConsts.CellWidth);
		int y = (int) (displacement.y / SerpentConsts.CellHeight);
		return this.maze.Cells[x,y];
	}
	
	public bool MotionBlocked(Vector3 position, SerpentConsts.Dir direction)
	{
		MazeCell cell = GetCellForPosition(position);
		if (cell == null) { return true; }
		MazeWall wall = cell.Walls[(int)direction];
		return (wall != null);
	}

	/// <summary>
	/// Gets the centre position for a cell side, to place a wall there
	/// </summary>
	/// <returns>The cell side's centre.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="side">Side.</param>
	private Vector3 GetCellSideCentre(int x, int y, int intSide)
	{
		float fx = (float)x + SerpentConsts.DirectionVector3[intSide].x * 0.5f;
		float fy = (float)y + SerpentConsts.DirectionVector3[intSide].y * 0.5f;

		Vector3 pos = GetCellCentre(fx, fy);
		return pos;
	}
}

