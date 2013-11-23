using System;
using UnityEngine;

public class SerpentUtils
{
	public Vector2 GetVector(char direction)
	{
		switch( direction )
		{
			case SerpentConsts.E:
				return new Vector2(1,0);
			case SerpentConsts.W:
				return new Vector2(-1,0);
			case SerpentConsts.N:
				return new Vector2(0,-1);
			case SerpentConsts.S:
				return new Vector2(0,1);
			default:
				break;
		}

		return new Vector2(0,0);
	}

}

