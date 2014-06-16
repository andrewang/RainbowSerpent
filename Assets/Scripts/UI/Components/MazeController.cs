using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MazeController : MonoBehaviour
{
	[SerializeField] public Maze Maze = null;
	[SerializeField] private GameObject wallSpritePrefab = null;
	[SerializeField] private GameObject screenShotPrefab = null;	
	[SerializeField] private UIPanel panel = null;
	[SerializeField] private ScreenShotTaker screenShotTaker = null; 
	
	private int levelNumber;
	private Color wallColour;
	
	private Action screenShotCompletedAction;
	private bool screenShotLoaded = false;
	
	private List<UISprite> wallSprites = new List<UISprite>();

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
		if (this.Maze == null) { Debug.LogError("GameSceneController: maze is null"); }
	}
	
	#endregion Verify Serialize Fields
	
	/// <summary>
	/// Sets up the maze controller
	/// </summary>
	/// <param name="mazeTextAsset">Maze text asset.</param>
	public void SetUp(int levelNum, TextAsset mazeTextAsset, Color wallColour)
	{
		this.levelNumber = levelNum;
		this.wallColour = wallColour;
		this.Maze.SetUp(mazeTextAsset);
		DetermineMazeScale();		
		CreateMazeSprites();
	}
		
	/// <summary>
	// DetermineSpriteScale compares the size of a cell with what is available on-screen and
	// determines how much the whole maze needs to be scaled down.
	// </summary>
	private void DetermineMazeScale()
	{
		Vector4 spriteBounds = this.panel.clipRange;
		// clip range is ZW.
		float maxWidth = spriteBounds.z;
		float maxHeight = spriteBounds.w;
	
		// add a bit to the dimensions so that the outside walls don't get clipped
		float q = maxWidth / ((this.Maze.Width + 0.2f) * SerpentConsts.CellWidth);
		float x = maxHeight / ((this.Maze.Height + 0.2f) * SerpentConsts.CellHeight);
		float scale = Mathf.Min(q, x);
		this.transform.localScale = new Vector3(scale, scale, 0.0f);
	}

	/// <summary>
	/// Creates the maze sprites.  These will be centred on the origin of the maze controller.
	/// </summary>
	private void CreateMazeSprites()
	{
		// If a screenshot for the level exists, then just load that?
		// DOORS need to be separate sprites.  So in order to create a screenshot, we would need to 
		// create the maze without doors.
		// The screenshot should be version-stamped so that when I update the version number, the old
		// screenshots are removed and new ones are generated
	
		int width = this.Maze.Width;
		int height = this.Maze.Height;
		if (width == 0 || height == 0) { return; }

		// Determine the lower left corner of the map.
		this.lowerLeftCellCentre.x = -1.0f * ((width - 1) * SerpentConsts.CellWidth) * 0.5f;
		this.lowerLeftCellCentre.y = -1.0f * ((height - 1) * SerpentConsts.CellHeight) * 0.5f;
		
		// If a screenshot for this level already exists then use that.
		/*
		if (ScreenShotExists())
		{
			LoadScreenShot();
			this.screenShotLoaded = true;
		}
		*/

		CreateHorizontalWallSprites();
		CreateVerticalWallSprites();
	}
	
	private void CreateHorizontalWallSprites()
	{
		int y = 0;
		int intSide = 0;
		
		//
		// In order to reduce the number of methods and prevent duplication of the code to loop through the rows/columns
		// to create walls, this and the CreateVerticalWallSprites method define funcs in the context of the values
		// y and intSide that can be passed into a method which does the looping.
		//
		Func<int, Wall> getWallFunction = delegate( int x )
		{
			MazeCell cell = this.Maze.Cells[x,y];			
			Wall wall = cell.Walls[intSide];
			return wall;		
		};
		
		//
		// This function creates a horizontal wall sprite stretching from startX to 
		// endX in maze coordinates.  
		//
		Func<int, int, UISprite> createSpriteFunction = delegate( int startX, int endX )
		{
			GameObject newWall = CreateWallObject();
			UISprite newWallSprite = GetUISpriteComponent(newWall);
			if (newWallSprite == null) 
			{ 
				return null; 
			}
			newWallSprite.color = this.wallColour;			
			
			// Configure as horizontal.
			newWallSprite.width = SerpentConsts.CellWidth * (endX - startX + 1);
			float x = (float)(startX + endX) * 0.5f;			
			Vector3 pos = GetCellSideCentre( x, y, intSide);
			pos.x -= newWallSprite.width * 0.5f;
			newWall.transform.localPosition = pos;	
			
			return newWallSprite;		
		};
		
		// Create south walls along all rows.
		intSide = (int) SerpentConsts.Dir.S;
		for (y = 0; y < this.Maze.Height; ++y)
		{
			CreateWalls(this.Maze.Width, getWallFunction, createSpriteFunction);
		}
	}
	
	
	private void CreateVerticalWallSprites()
	{
		int x = 0;
		int intSide = 0;
		
		//
		// In order to reduce the number of methods and prevent duplication of the code to loop through the rows/columns
		// to create walls, this and the CreateVerticalWallSprites method define funcs in the context of the values
		// y and intSide that can be passed into a method which does the looping.
		//
		Func<int, Wall> getWallFunction = delegate( int y )
		{
			MazeCell cell = this.Maze.Cells[x,y];			
			Wall wall = cell.Walls[intSide];
			return wall;		
		};
		
		//
		// This function creates a vertical wall sprite stretching from startY to 
		// endY in maze coordinates.  
		//
		Func<int, int, UISprite> createSpriteFunction = delegate( int startY, int endY )
		{
			GameObject newWall = CreateWallObject();
			UISprite newWallSprite = GetUISpriteComponent(newWall);
			if (newWallSprite == null) 
			{ 
				return null; 
			}
			newWallSprite.color = this.wallColour;			
			
			newWallSprite.width = SerpentConsts.CellHeight * (endY - startY + 1);
			
			float y = (float)(startY + endY) * 0.5f;
			Vector3 pos = GetCellSideCentre( x, y, intSide);			
			pos.y -= newWallSprite.width * 0.5f;
			newWall.transform.localPosition = pos;			

			// Rotate the sprite to be vertical
			newWallSprite.transform.Rotate(Vector3.forward * 90.0f);
			
			return newWallSprite;		
		};
		
		// Create east walls along all columns.
		intSide = (int) SerpentConsts.Dir.E;
		for (x = 0; x < this.Maze.Width; ++x)
		{
			CreateWalls(this.Maze.Height, getWallFunction, createSpriteFunction);
		}
	}
	
	/// <summary>
	/// Creates walls given certain specifications and actions.
	/// </summary>
	/// <param name="loopLimit">Loop limit.</param>
	/// <param name="getWallAction">Get wall action.</param>
	/// <param name="createSpriteFunction">Create sprite action.</param>
	private void CreateWalls(int loopLimit, Func<int, Wall> getWallFunction, Func<int, int, UISprite> createSpriteFunction)
	{
		int start = -1;
		for (int loop = 0; loop < loopLimit; ++loop)
		{
			Wall wall = getWallFunction(loop);
			bool wallEnds = (start >= 0 && (wall == null || wall is Door));
			if (wallEnds && !this.screenShotLoaded)
			{
				UISprite sprite = createSpriteFunction(start, loop - 1);
				this.wallSprites.Add(sprite);
				start = -1;			
			}
			if (wall is Door)
			{
				Door door = wall as Door;
				// Create a sprite for the door.
				UISprite doorSprite = createSpriteFunction(loop, loop);	
				door.Sprite = doorSprite;			
			}
			else if (wall != null && start == -1)
			{
				// no wall start is recorded, so record the start of one
				start = loop;
			}
		}
		
		// Any final wall at the end?
		if (start != -1 && !this.screenShotLoaded)
		{
			UISprite sprite = createSpriteFunction(start, loopLimit - 1);
			this.wallSprites.Add(sprite);
		}
	}
	
	/// <summary>
	/// Creates a wall object with default parentage and scale
	/// </summary>
	/// <returns>The new wall object.</returns>
	private GameObject CreateWallObject()
	{
		GameObject newWall = (GameObject) Instantiate(this.wallSpritePrefab, new Vector3(0,0,0), Quaternion.identity);
		newWall.transform.parent = this.transform;
		newWall.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		return newWall;
	}
	
	/// <summary>
	/// Gets the sprite component from a newly created game object.
	/// </summary>
	/// <returns>The sprite.</returns>
	/// <param name="obj">Object.</param>
	private UISprite GetUISpriteComponent(GameObject obj)
	{
		UISprite sprite = obj.GetComponent<UISprite>();
		if (sprite == null) { return null; }
		
		return sprite;
	}
	
	//screenShotPrefab
	
	/// <summary>
	/// Creates a container object for a screenshot with default parentage and scale
	/// </summary>
	/// <returns>The new wall object.</returns>
	private GameObject CreateScreenshotObject()
	{
		GameObject newObj = (GameObject) Instantiate(this.screenShotPrefab, new Vector3(0,0,0), Quaternion.identity);
		newObj.transform.parent = this.transform;
		newObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		newObj.transform.localPosition = new Vector3(0,0);
		return newObj;
	}
	
	/// <summary>
	/// Gets the texture component from a newly created game object.
	/// </summary>
	/// <returns>The texture.</returns>
	/// <param name="obj">Object.</param>
	private UITexture GetUITextureComponent(GameObject obj)
	{
		UITexture texture = obj.GetComponent<UITexture>();
		if (texture == null) { return null; }
		
		return texture;
	}
		
	#region Screenshots
	
	// A couple of screenshots can replace many wall sprites!  One for the outside walls and snake entry points, and
	// one for everything.	
	
	public void CreateScreenshot(Action completedAction)
	{
		if (ScreenShotExists ()) 
		{
			completedAction();
			return;
		}
		
		this.screenShotCompletedAction = completedAction;
		
		this.Maze.HideDoors();
		
		Vector3 relativePosition = GetLocalPositionSum( this.screenShotTaker.transform );
		                                               
		this.screenShotTaker.TakeScreenShot(ScreenShotPath(), 
			(int) this.panel.clipRange.z, (int) this.panel.clipRange.w, 
		                                    (int) relativePosition.x, (int) relativePosition.y, ScreenShotCreated);
	}
	
	private void ScreenShotCreated(Texture2D screenShotTexture)
	{
		UseScreenShot(screenShotTexture);
		if (this.screenShotCompletedAction != null)
		{
			this.screenShotCompletedAction();
			this.screenShotCompletedAction = null;
		}
		
	}
	
	private void LoadScreenShot()
	{
		string path = ScreenShotPath();
		byte[] byteArray = File.ReadAllBytes(path);
		
		Texture2D screenshotTexture = new Texture2D(4, 4);
		screenshotTexture.LoadImage(byteArray);
		
		UseScreenShot(screenshotTexture);
	}
	
	private void UseScreenShot(Texture2D screenShotTexture)
	{			
		this.Maze.ShowDoors();
		
		GameObject gameObj = CreateScreenshotObject();
		UITexture uiTexture = GetUITextureComponent(gameObj);		
		if (uiTexture == null) { return; }		
								
		uiTexture.mainTexture = screenShotTexture;
		uiTexture.MakePixelPerfect();	
				
		// TODO: it seems that accessing the transform during a coroutine callback is unsafe.	
		
		// NOTE: the maze itself is being shown at a reduced scale.  The screenshot was taken of images which had
		// already been scaled down.  So at this point we need to divide by the maze's scale to compensate for the
		// application of the current maze object's scale value on the ALREADY scaled-down maze.
		Vector3 mazeScale = this.transform.localScale;
		Vector3 textureScale = gameObj.transform.localScale;
		textureScale.x /= mazeScale.x;
		textureScale.y /= mazeScale.y;
		gameObj.transform.localScale = textureScale;
		
		// set z position of the screenshot to further back.
		int depth = uiTexture.depth;
		uiTexture.depth = depth - 10;
		
		RemoveExistingWallSprites();
	}
	
	private void RemoveExistingWallSprites()
	{
		// Remove existing wall sprites after creating a screenshot
		foreach(UISprite sprite in this.wallSprites)
		{
			sprite.gameObject.transform.parent = null;
			UnityEngine.Object.Destroy(sprite.gameObject);
		}
		this.wallSprites.Clear();
	}
	
	// In the chain of transforms, calculate all the local offsets relative to the specified ancestor transform.
	private Vector3 GetLocalPositionSum(Transform ancestorTransform)
	{
		Vector3 relativePosition = this.transform.localPosition;
		Transform temp = this.transform.parent;
		
		while( temp != ancestorTransform && temp != null )
		{
			relativePosition += temp.localPosition;
			temp = temp.parent;
		}
		
		return relativePosition;
	}
	
	private string ScreenShotPath()
	{
		string dataPath = Application.dataPath;
		dataPath = Path.Combine(dataPath, "Maze." + this.levelNumber + ".png");
		return dataPath;
	}
	
	private bool ScreenShotExists()
	{
		string path = ScreenShotPath();
		return File.Exists(path);
	}
	
	#endregion Screenshots


	#region Movement related methods	
	
	public void OpenDoor(Vector3 position, SerpentConsts.Dir direction)
	{
		MazeCell cell = GetCellForPosition(position);
		if (cell == null) { return; }
		
		Wall w = cell.Walls[ (int) direction ];
		if (w is Door)
		{
			Door d = w as Door;
			d.Open();
		}
	}
	
	public void CloseDoor(Vector3 position, SerpentConsts.Dir direction)
	{
		MazeCell cell = GetCellForPosition(position);
		if (cell == null) { return; }
		
		Wall w = cell.Walls[ (int) direction ];
		if (w is Door)
		{
			Door d = w as Door;
			d.Close();
		}
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
		if (cell.X < this.Maze.Width)
		{
			availableDirections.Add( SerpentConsts.Dir.E ); 
		}
		if (cell.Y > 0)
		{
			availableDirections.Add( SerpentConsts.Dir.S );
		}
		if (cell.Y < this.Maze.Height)
		{
			availableDirections.Add( SerpentConsts.Dir.N );
		}
		
		return availableDirections;
	}
	
	#endregion Motion methods
	
	#region Snake placement
	
	public void PlaceSnake(Snake snake, bool player)
	{
		IntVector2 cellPosition;
		SerpentConsts.Dir direction;
		if (player)
		{
			cellPosition = this.Maze.PlayerStartPosition;
			direction = this.Maze.PlayerStartFacing;
		}
		else
		{
			cellPosition = this.Maze.EnemyStartPosition;
			direction = this.Maze.EnemyStartFacing;
		}
		
		Vector3 position = this.GetCellCentre(cellPosition.x, cellPosition.y);
		Debug.Log("Adding snake at (" + cellPosition.x + "," + cellPosition.y + "): " + position.x + "," + position.y);
		snake.SetInitialLocation(position, direction);
		snake.Visible = true;
		snake.Controller.StartMoving(direction);
		// TODO Should something be added to the snake trail?
	}
	
	#endregion Snake placement
	
	#region Utility methods
	
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
		if (x < 0 || y < 0 || x >= this.Maze.Width || y >= this.Maze.Height) { return null; }
		return this.Maze.Cells[x,y];
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
	
	#endregion Utility methods
	
}

