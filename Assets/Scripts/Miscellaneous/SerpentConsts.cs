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
	public const int N = 0;
	public const int E = 1;
	public const int S = 2;
	public const int W = 3;

	public const int CellWidth = 64;
	public const int CellHeight = 48;
	//public const int MazeWidth = 10;
	//public const int MazeHeight = 14;

	public static Dictionary<char,int> DirectionIndexes = new Dictionary<char,int>()
	{
		{'n', SerpentConsts.N},
		{'e', SerpentConsts.E},
		{'s', SerpentConsts.S},
		{'w', SerpentConsts.W},
		{'N', SerpentConsts.N},
		{'E', SerpentConsts.E},
		{'S', SerpentConsts.S},
		{'W', SerpentConsts.W}
	};

	public static IntVector2[] DirectionVector = new IntVector2[]
	{
		new IntVector2( 0,  1),
		new IntVector2( 1,  0),
		new IntVector2( 0, -1),
		new IntVector2(-1,  0)
	};

	public static Vector3[] DirectionVector3 = new Vector3[]
	{
		new Vector3( 0,  1, 0),
		new Vector3( 1,  0, 0),
		new Vector3( 0, -1, 0),
		new Vector3(-1,  0, 0)
	};

	public static int[] OppositeDirection = new int[]
	{
		SerpentConsts.S,
		SerpentConsts.W,
		SerpentConsts.N,
		SerpentConsts.E
	};

	public static class SceneNames
	{
		public const string Loading = "LoadingScene";
		public const string Game = "GameScene";
		public const string MainMenu = "MenuScene";
	};
}

