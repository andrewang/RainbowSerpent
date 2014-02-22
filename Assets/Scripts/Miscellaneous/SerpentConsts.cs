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
	public const string XKey = "x";
	public const string YKey = "y";
	public const string DirectionKey = "dir";
	
	// Pixel sizes of cells.
	public const int CellWidth = 64;
	public const int CellHeight = 48;
	
	public enum Dir
	{
		N = 0,
		E,
		S,
		W,
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
	
	public static TimeSpan PlayerEggFrequency = new TimeSpan(0, 0, 45);
	public static TimeSpan EnemyEggFrequency = new TimeSpan(0, 0, 5);
	public static TimeSpan TimeToLayEgg = new TimeSpan(0, 0, 10);
	public static TimeSpan EnemyEggHatchingTime = new TimeSpan(0, 0, 30);
	
	public static int MaxNumEnemySnakes = 3;
	public static int NormalEnemySnakeLength = 5;
	public static int SmallEnemySnakeLength = 3;
	public static int PlayerSnakeLength = 3;
	
	public static class SceneNames
	{
		public const string Loading = "LoadingScene";
		public const string Game = "GameScene";
		public const string MainMenu = "MenuScene";
	};
}

