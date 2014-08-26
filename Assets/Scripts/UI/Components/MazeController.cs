using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Serpent;
using SerpentExtensions;

public class MazeController : MonoBehaviour
{
	[SerializeField] public Maze Maze = null;
	
	[SerializeField] private GameObject wallSpritePrefab = null;
	[SerializeField] private GameObject[] cornerPrefabs = new GameObject[0];
	
	[SerializeField] private GameObject screenShotPrefab = null;	
	[SerializeField] private UIPanel panel = null;
	[SerializeField] private bool useScreenShots = true;
	[SerializeField] private ScreenShotTaker screenShotTaker = null; 
	[SerializeField] private ScreenShotFileManager screenShotFileManager = null;
	
	private int levelNumber;
	private Color wallColour;
	
	private GameObject screenShotContainer;
	private Action screenShotCompletedAction;
	private bool screenShotLoaded = false;
	
	private bool scaleSet = false;
	
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
		CreateMazeSprites();
	}
	
	public void Reset()
	{
		this.screenShotContainer = null;
		this.screenShotCompletedAction = null;
		this.screenShotLoaded = false;
		this.scaleSet = false;
		this.lowerLeftCellCentre = new Vector3(0,0,0);
		
		Debug.Log ("Destroying stuff...");
		RemoveExistingWallSprites();
		foreach( Transform child in this.Maze.transform )
		{
			Debug.Log ("This should destroy " + child.gameObject.name);
			Destroy(child.gameObject);
		}
	}
	
	void LateUpdate()
	{
		if (this.scaleSet == false)
		{
			// Check for a resize component on the panel and make it run FIRST.
			ResizePanel resizeScript = this.panel.GetComponent<ResizePanel>();
			if (resizeScript != null)
			{
				resizeScript.ResizeThePanel();
			}
		
			DetermineMazeScale();
			this.scaleSet = true;
			
			if (this.useScreenShots == false) { return; }
			
			if (this.screenShotContainer == null)
			{
				CreateScreenshot(this.screenShotCompletedAction);
			}
			else
			{
				SetScreenShotScale();				
			}
		}
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
		// DOORS need to be separate sprites.  So in order to create a screenshot, we would need to 
		// create the maze without doors.
		
		int width = this.Maze.Width;
		int height = this.Maze.Height;
		if (width == 0 || height == 0) { return; }

		// Determine the lower left corner of the map.
		this.lowerLeftCellCentre.x = -1.0f * ((width - 1) * SerpentConsts.CellWidth) * 0.5f;
		this.lowerLeftCellCentre.y = -1.0f * ((height - 1) * SerpentConsts.CellHeight) * 0.5f;
		
		// If a screenshot for this level already exists then use that.
		if (this.useScreenShots && this.screenShotFileManager.ScreenShotExists(this.levelNumber))
		{
			Texture2D screenShotTexture = this.screenShotFileManager.LoadScreenShot(this.levelNumber);		
			UseScreenShot(screenShotTexture);
			this.screenShotLoaded = true;
		}

		CreateHorizontalWallSprites();
		CreateVerticalWallSprites();
		
		if (this.screenShotLoaded == false)
		{	
			CreateCornerSprites();
		}
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
			newWallSprite.width = SerpentConsts.CellWidth * (endX - startX + 1) - 7; // + SerpentConsts.WallIntersectionOverlap * 2;
			float x = (float)(startX + endX) * 0.5f;			
			Vector3 pos = GetCellSideCentre( x, y, intSide);
			//pos.x -= newWallSprite.width * 0.5f;
			newWall.transform.localPosition = pos;	
			
			return newWallSprite;		
		};
		
		// Create south walls along all rows.
		intSide = (int) Direction.S;
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
			
			newWallSprite.width = SerpentConsts.CellHeight * (endY - startY + 1) - 7; // + SerpentConsts.WallIntersectionOverlap * 2;
			
			float y = (float)(startY + endY) * 0.5f;
			Vector3 pos = GetCellSideCentre( x, y, intSide);			
			//pos.y -= newWallSprite.width * 0.5f;
			newWall.transform.localPosition = pos;			

			// Rotate the sprite to be vertical
			newWallSprite.transform.Rotate(Vector3.forward * 90.0f);
			
			return newWallSprite;		
		};
		
		// Create east walls along all columns.
		intSide = (int) Direction.E;
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
			bool wallEnds = (start >= 0); // && (wall == null || wall is Door));
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
	
	// Loop through all intersections of walls
	// Where there is an end or a corner, add a corner graphic.
	private void CreateCornerSprites()
	{
		// How iteration for creating *corners* works:
		// We are iterating on squares (not corners).  At each cell, we will check to see if we
		// should create a corner graphic in the UPPER RIGHT of the cell.
		for (int y = 0; y < this.Maze.Height - 1; ++y)
		{
			for (int x = 0; x < this.Maze.Width - 1; ++x)
			{
				CreateCornerSpriteAt(x, y);
			}
		}
	}
	
	private void CreateCornerSpriteAt(int x, int y)
	{
		Wall northWall;
		Wall southWall;
		Wall westWall;
		Wall eastWall;
		
		MazeCell cell = this.Maze.Cells[x,y];
		southWall = cell.Walls[(int) Direction.E];
		westWall = cell.Walls[(int) Direction.N];
		
		MazeCell swCell = this.Maze.Cells[x+1,y+1];
		northWall = swCell.Walls[(int) Direction.W];
		eastWall = swCell.Walls[(int) Direction.S];
		
		Wall[] walls = new Wall[(int)Direction.Count];
		walls[(int)Direction.N] = northWall;
		walls[(int)Direction.S] = southWall;
		walls[(int)Direction.W] = westWall;
		walls[(int)Direction.E] = eastWall;
		
		int numWalls = 0;
		for (int i = 0; i < (int)Direction.Count; ++i)
		{
			if (walls[i] != null)
			{
				++numWalls;
			}
		}
		
		if (numWalls == 0)
		{
			return;
		}
		
		Vector3 cornerLocation = GetUpperRightCorner((float)x, (float)y);
		
		GameObject prefab = this.cornerPrefabs[numWalls - 1];
		Direction orientation = Direction.N;
		if (numWalls == 1)
		{
			// end piece
			for (int i = 0; i < (int)Direction.Count; ++i)
			{
				if (walls[i] != null)
				{
					// We need the corner to be opposite to this direction to meet the wall graphic.
					orientation = SerpentConsts.OppositeDirection[i];
					//Debug.Log("Wall position is " + walls[i].transform.localPosition);
				}
			}
		}
		else if (numWalls == 2)
		{
			// elbow or through piece
			if ((walls[(int)Direction.N] != null && walls[(int)Direction.S] != null) || (walls[(int)Direction.W] != null && walls[(int)Direction.E] != null))
			{
				// through piece uses the wall sprite instead of a corner
				prefab = this.wallSpritePrefab;
				if (walls[(int)Direction.W] != null)		
				{
					orientation = Direction.N;
				}
				else
				{
					orientation = Direction.W;
				}
			}
			else
			{
				prefab = this.cornerPrefabs[1];
				if (walls[(int)Direction.W] != null)
				{
					if (walls[(int)Direction.N] != null)
					{
						// W and N
						orientation = Direction.S;
					}
					else
					{
						// W and S
						orientation = Direction.E;
					}
				}
				else
				{
					if (walls[(int)Direction.N] != null)
					{
						// E and N
						orientation = Direction.W;
					}
					else
					{
						// W and S
						orientation = Direction.N;
					}
				}
			}
			
		}
		else if (numWalls == 3)
		{
			// T intersection
			// The T corner graphic is oriented to match the missing wall if the missing wall is in the north.
			if (walls[(int)Direction.S] == null)
			{
				orientation = Direction.S;
			}
			else if (walls[(int)Direction.W] == null)
			{
				orientation = Direction.W;
			}
			else if (walls[(int)Direction.E] == null)
			{
				orientation = Direction.E;
			}
			
		}
		else
		{
			// + intersection
			// Works in all directions.
		}
		
		if (prefab == null) { return; }
		
		GameObject instantiated = (GameObject) Instantiate(prefab, new Vector3(0,0,0), Quaternion.identity);
		instantiated.transform.parent = this.transform;
		instantiated.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
		instantiated.transform.localPosition = cornerLocation;
		instantiated.transform.localEulerAngles = SerpentConsts.RotationVector3[(int)orientation];
		
		UISprite sprite = instantiated.GetComponent<UISprite>();
		sprite.color = this.wallColour;
		
		// Special handling to set the length of through pieces.
		if (prefab == this.wallSpritePrefab)
		{
			// get the size of the corner prefab
			UISprite cornerSprite = this.cornerPrefabs[0].GetComponent<UISprite>();
			sprite.width = cornerSprite.width;
			sprite.height = cornerSprite.height;
		}
		
		this.wallSprites.Add(sprite);
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
		// For some reason screenshots seem to be shown slightly versus the real thing
		newObj.transform.localPosition = new Vector3(0.0f,0.0f,0.0f);
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
		if (this.useScreenShots == false)
		{	
			// just run what was supposed to be executed after creating the screenshot
			if (completedAction != null)
			{
				completedAction();
			}
			return;
		}
		
		if (this.screenShotFileManager.ScreenShotExists(this.levelNumber)) 
		{
			// screenshot should already be attached to the UI
			if (completedAction != null)
			{
				completedAction();
			}
			return;
		}
		
		this.screenShotCompletedAction = completedAction;
		if (this.scaleSet == false)
		{
			// it is too early to take a screenshot
			return;
		}
		
		this.Maze.HideDoors();
		
		Vector3 relativePosition = this.transform.GetLocalPositionRelativeTo( this.screenShotTaker.transform );
		
		float screenRescaling = Managers.ScreenManager.Scale;
		int shotWidth = (int) (this.panel.clipRange.z / screenRescaling);
		int shotHeight = (int) (this.panel.clipRange.w / screenRescaling);
		
		this.screenShotTaker.TakeScreenShot(this.screenShotFileManager.ScreenShotPath(this.levelNumber), 
		                                    shotWidth, shotHeight, 
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
	
	private void UseScreenShot(Texture2D screenShotTexture)
	{			
		this.Maze.ShowDoors();
		
		GameObject gameObj = CreateScreenshotObject();
		UITexture uiTexture = GetUITextureComponent(gameObj);		
		if (uiTexture == null) { return; }		
								
		uiTexture.mainTexture = screenShotTexture;
		uiTexture.MakePixelPerfect();	
		
		this.screenShotContainer = gameObj;
			
		// set z position of the screenshot to further back.
		int depth = uiTexture.depth;
		uiTexture.depth = depth - 10;
		
		RemoveExistingWallSprites();
		
		if (this.scaleSet)
		{
			SetScreenShotScale();
		}
	}
	
	private void SetScreenShotScale()
	{
		if (this.screenShotContainer == null) { return; }

		float screenRescaling = Managers.ScreenManager.Scale;
		
		Vector3 mazeScale = this.transform.localScale;
		Vector3 textureScale = this.screenShotContainer.transform.localScale;
		textureScale.x /= mazeScale.x;
		textureScale.y /= mazeScale.y;
		textureScale *= screenRescaling;
		this.screenShotContainer.transform.localScale = textureScale;
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
	
	// In the chain of transforms, calculate all the local offsets relative to the specified transform.
	private Vector3 GetSumOfLocalPositions(Transform topMostTransform)
	{
		Vector3 relativePosition = this.transform.localPosition;
		Transform temp = this.transform.parent;
		
		while( temp != topMostTransform && temp != null )
		{
			relativePosition += temp.localPosition;
			temp = temp.parent;
		}
		
		return relativePosition;
	}
	
	#endregion Screenshots


	#region Movement related methods	
	
	public void OpenDoor(Vector3 position, Direction direction)
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
	
	public void CloseDoor(Vector3 position, Direction direction)
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
	public bool IsMotionBlocked(Vector3 position, Direction direction)
	{
		MazeCell cell = GetCellForPosition(position);
		if (cell == null) { return true; }
		
		return cell.IsMotionBlocked(direction);
	}
	
	public List<Direction> GetValidDirections(Vector3 position, bool allowOffscreen)
	{
		List<Direction> availableDirections = new List<Direction>();
		if (allowOffscreen)
		{
			for (Direction dir = Direction.First; dir <= Direction.Last; ++dir)
			{
				availableDirections.Add(dir);
			}		
			return availableDirections;
		}
		
		MazeCell cell = GetCellForPosition(position);
		
		if (cell.X > 0) 
		{
			availableDirections.Add( Direction.W ); 
		}
		if (cell.X < this.Maze.Width)
		{
			availableDirections.Add( Direction.E ); 
		}
		if (cell.Y > 0)
		{
			availableDirections.Add( Direction.S );
		}
		if (cell.Y < this.Maze.Height)
		{
			availableDirections.Add( Direction.N );
		}
		
		return availableDirections;
	}
	
	#endregion Motion methods
	
	#region Snake placement
	
	public void PlaceSnake(Snake snake, bool player)
	{
		IntVector2 cellPosition;
		Direction direction;
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
		snake.SetInitialLocation(position, direction);
		snake.Visible = true;
		snake.Controller.StartMoving(direction);		
	}
	
	#endregion Snake placement
	
	#region Utility methods
	
	/// <summary>
	/// Gets the centre position of a cell
	/// </summary>
	/// <returns>The cell centre.</returns>
	/// <param name="x">The x coordinate (cells).</param>
	/// <param name="y">The y coordinate (cells).</param>
	// TODO move this method into MazeCell if possible. Or make it take a MazeCell or IntVector2 instead.
	public Vector3 GetCellCentre(float x, float y)
	{
		Vector3 pos = this.lowerLeftCellCentre;
		pos.x += x * SerpentConsts.CellWidth;
		pos.y += y * SerpentConsts.CellHeight;
		return pos;
	}
	
	
	private Vector3 GetUpperRightCorner(float x, float y)
	{
		Vector3 pos = GetCellCentre( x + 0.5f, y + 0.5f);	
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
	
	public Vector3 GetNextCellCentre(Vector3 position, Direction direction)
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
	
	#region Debug methods
	
	public UISprite GetFirstWallSprite()
	{
		if (this.wallSprites.Count == 0) { return null; }
		return this.wallSprites[0];
	}
	
	public GameObject GetScreenShot()
	{
		return this.screenShotContainer;
	}
	#endregion Debug methods
	
}

