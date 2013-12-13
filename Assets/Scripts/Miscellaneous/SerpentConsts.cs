using System;
using System.Collections.Generic;
using UnityEngine;

public static class SerpentConsts
{
	public const int NumDirections = 4; 

	public const string WidthKey = "width";
	public const string HeightKey = "height";
	public const string CellsKey = "cells";
	public const string WallsKey = "walls";
	public const string XKey = "x";
	public const string YKey = "y";

	public const int CellWidth = 64;
	public const int CellHeight = 48;
	//public const int MazeWidth = 10;
	//public const int MazeHeight = 14;
	
	public enum Dir
	{
		N = 0,
		E,
		S,
		W,
		None
	}

	public static Dictionary<char,SerpentConsts.Dir> DirectionIndexes = new Dictionary<char,SerpentConsts.Dir>()
	{
		{'n', SerpentConsts.Dir.N},
		{'e', SerpentConsts.Dir.E},
		{'s', SerpentConsts.Dir.S},
		{'w', SerpentConsts.Dir.W},
		{' ', SerpentConsts.Dir.None},
		{'N', SerpentConsts.Dir.N},
		{'E', SerpentConsts.Dir.E},
		{'S', SerpentConsts.Dir.S},
		{'W', SerpentConsts.Dir.W}
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

	public static class SceneNames
	{
		public const string Loading = "LoadingScene";
		public const string Game = "GameScene";
		public const string MainMenu = "MenuScene";
	};
}

