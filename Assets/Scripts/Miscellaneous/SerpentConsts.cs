using System;
using System.Collections.Generic;
using UnityEngine;
using Serpent;

public static class SerpentConsts
{
	public const string Version = "0.2.0";
	
	#region Level Data Keys
	
	// Keys for reading level data	
	public const string WidthKey = "width";
	public const string HeightKey = "height";
	public const string CellsKey = "cells";
	public const string WallsKey = "walls";
	public const string DoorsKey = "doors";
	public const string PlayerStartZoneKey = "playerStartZone";
	public const string XKey = "X";
	public const string YKey = "Y";
	public const string DirectionKey = "dir";
	
	#endregion Level Data Keys
	
	#region Map Constants
	
	// Pixel sizes of cells.
	public const int CellWidth = 32;
	public const int CellHeight = 24;	

	public const int BorderWidth = 1;
	
	#endregion Map Constants

	#region Direction-related
	
	public static Dictionary<char,List<Direction>> DirectionIndexes = new Dictionary<char,List<Direction>>()
	{
		{'n', new List<Direction>{Direction.N}},
		{'e', new List<Direction>{Direction.E}},
		{'s', new List<Direction>{Direction.S}},
		{'w', new List<Direction>{Direction.W}},
		{'N', new List<Direction>{Direction.N}},
		{'E', new List<Direction>{Direction.E}},
		{'S', new List<Direction>{Direction.S}},
		{'W', new List<Direction>{Direction.W}},
		
		{'f', new List<Direction>{Direction.N, Direction.W}},
		{'g', new List<Direction>{Direction.N, Direction.E}},
		{'b', new List<Direction>{Direction.S, Direction.E}},
		{'v', new List<Direction>{Direction.S, Direction.W}},
		
		{'F', new List<Direction>{Direction.N, Direction.W}},
		{'G', new List<Direction>{Direction.N, Direction.E}},
		{'B', new List<Direction>{Direction.S, Direction.E}},
		{'V', new List<Direction>{Direction.S, Direction.W}},
		
	};
	
	public static char[] DirectionChar = new char[]
	{
		'N',
		'E',
		'S',
		'W'
	};

	public static IntVector2[] DirectionVector = new IntVector2[]
	{
		new IntVector2( 0,  1),
		new IntVector2( 1,  0),
		new IntVector2( 0, -1),
		new IntVector2(-1,  0),
		new IntVector2( 0,  0)
	};

	public static Vector3[] DirectionVector3 = new Vector3[]
	{
		new Vector3( 0,  1, 0),
		new Vector3( 1,  0, 0),
		new Vector3( 0, -1, 0),
		new Vector3(-1,  0, 0),
		new Vector3( 0,  0, 0)
	};
	
	public static Vector3[] RotationVector3 = new Vector3[]
	{
		new Vector3( 0, 0, 0 ),
		new Vector3( 0, 0, -90 ),
		new Vector3( 0, 0, -180 ),
		new Vector3( 0, 0, 90 ),
		new Vector3( 0, 0, 0 )		
	};

	public static Direction[] OppositeDirection = new Direction[]
	{
		Direction.S,
		Direction.W,
		Direction.N,
		Direction.E,
		Direction.None		
	};
	
	public static Direction GetDirectionForVector( Vector3 v )
	{
		for(int i = 0; i < SerpentConsts.DirectionVector3.Length; ++i)
		{
			if (v == SerpentConsts.DirectionVector3[i])
			{
				return (Direction)i;
			}
		}
		
		return Direction.None;
	}
	
	#endregion Direction-related
	
	#region Eggs
	
	public static float PlayerEggFrequency = 60.0f;
	public static float EnemyEggFrequency = 10.0f;
	public static float TimeToLayEgg = 10.0f;
	public static float EnemyEggHatchingTime = 20.0f;
	public static float FrogRespawnDelay = 10.0f;
	
	public static float GetEggLayingFrequency( Side side )
	{
		if (side == Side.Player)
		{
			return PlayerEggFrequency;
		}
		else
		{
			return EnemyEggFrequency;
		}
	}
	
	#endregion Eggs
	
	#region Snake Lengths
	
	public static int GetNewlyHatchedSnakeLength( Side side )
	{
		if (side == Side.Player)
		{
			return SmallPlayerSnakeLength;
		}
		else
		{
			return SmallEnemySnakeLength;
		}
	}
	
	public static int InitialNumPlayerSnakes = 3;
	public static int MaxNumEnemySnakes = 3;
	public static int EnemySnakeLength = 5;
	public static int SmallEnemySnakeLength = 3;
	public static int PlayerSnakeLength = 3;
	public static int SmallPlayerSnakeLength = 2;
	
	#endregion Snake Lengths
	
	#region Score
	
	public static int ScoreForBonusLife = 10000;
	
	#endregion Score
	
	#region Player Snake Colours
		
	public static Color[] PlayerSegmentColours = new Color[]
	{
		Color.green,	//0f, 1.0f, 0f
		new Color		(0f, 1.0f, 1.0f),
		Color.blue,		//0f, 0f, 1.0f
		new Color		(1.0f, 0f, 1.0f),
		Color.red,		//1.0f, 0f, 0f
		Color.yellow,
	};
	
	#endregion Player Snake Colours
	
	#region Scene Names
	
	public static class SceneNames
	{
		public const string Loading = "LoadingScene";
		public const string Game = "GameScene";
		public const string Help = "HelpScene";
		public const string Credits = "CreditsScene";		
		public const string Main = "MainMenuScene";
	};
	
	#endregion Scene Names
}

