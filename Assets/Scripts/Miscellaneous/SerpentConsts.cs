using System;
using System.Collections.Generic;
using UnityEngine;

public static class SerpentConsts
{
	public const int NumDirections = 4; 

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
	
	// Pixel sizes of cells.
	public const int CellWidth = 32;
	public const int CellHeight = 24;
	
	public const int BorderWidth = 1;
	
	public enum Dir
	{
		N = 0,
		E,
		S,
		W,
		First = N,
		Last = W,
		None
	}

	public static Dictionary<char,List<SerpentConsts.Dir>> DirectionIndexes = new Dictionary<char,List<SerpentConsts.Dir>>()
	{
		{'n', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N}},
		{'e', new List<SerpentConsts.Dir>{SerpentConsts.Dir.E}},
		{'s', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S}},
		{'w', new List<SerpentConsts.Dir>{SerpentConsts.Dir.W}},
		{'N', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N}},
		{'E', new List<SerpentConsts.Dir>{SerpentConsts.Dir.E}},
		{'S', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S}},
		{'W', new List<SerpentConsts.Dir>{SerpentConsts.Dir.W}},
		
		{'f', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N, SerpentConsts.Dir.W}},
		{'g', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N, SerpentConsts.Dir.E}},
		{'b', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S, SerpentConsts.Dir.E}},
		{'v', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S, SerpentConsts.Dir.W}},
		
		{'F', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N, SerpentConsts.Dir.W}},
		{'G', new List<SerpentConsts.Dir>{SerpentConsts.Dir.N, SerpentConsts.Dir.E}},
		{'B', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S, SerpentConsts.Dir.E}},
		{'V', new List<SerpentConsts.Dir>{SerpentConsts.Dir.S, SerpentConsts.Dir.W}},
		
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

	public static SerpentConsts.Dir[] OppositeDirection = new SerpentConsts.Dir[]
	{
		SerpentConsts.Dir.S,
		SerpentConsts.Dir.W,
		SerpentConsts.Dir.N,
		SerpentConsts.Dir.E,
		SerpentConsts.Dir.None		
	};
	
	public static SerpentConsts.Dir GetDirectionForVector( Vector3 v )
	{
		for(int i = 0; i < DirectionVector3.Length; ++i)
		{
			if (v == DirectionVector3[i])
			{
				return (SerpentConsts.Dir)i;
			}
		}
		
		return SerpentConsts.Dir.None;
	}
	
	public enum Side
	{
		Player,
		Enemy,
		Frog,
	}
	
	public enum LevelState
	{
		None,
		LevelStart,
		Playing,
		LevelEnd
	}
	
	
	public static float PlayerEggFrequency = 6.0f;
	public static float EnemyEggFrequency = 15.0f;
	public static float TimeToLayEgg = 10.0f;
	public static float EnemyEggHatchingTime = 30.0f;
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
	public static int PlayerSnakeLength = 6;
	public static int SmallPlayerSnakeLength = 2;
	
	public static int ScoreForBonusLife = 10000;
		
	public static Color[] PlayerSegmentColours = new Color[]
	{
		Color.green,	//0f, 1.0f, 0f
		new Color		(0f, 1.0f, 1.0f),
		Color.blue,		//0f, 0f, 1.0f
		new Color		(1.0f, 0f, 1.0f),
		Color.red,		//1.0f, 0f, 0f
		Color.yellow,
	};
	
	
	public static class SceneNames
	{
		public const string Loading = "LoadingScene";
		public const string Game = "GameScene";
		public const string MainMenu = "MenuScene";
	};
}

