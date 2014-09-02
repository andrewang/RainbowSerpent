using UnityEngine;
using Serpent;

public class DifficultySettings : MonoBehaviour
{
	public float GameSpeedMultiplier;
	public float GameSpeedIncreasePerLevel;
	
	public float BasePlayerSpeed;
	public float PlayerSegmentSlowdown;
	public float PlayerInitialEggLayingDelay;
	public float PlayerEggLayingDelay;
	public float PlayerEggCreationTime;
	
	public float BaseEnemySpeed;
	public float EnemySegmentSlowdown;
	public float EnemyEggLayingDelay;
	public float EnemyEggCreationTime;
	public float EnemyEggHatchingTime;
	
	public float FrogRespawnDelay;
	public float FrogJumpingSpeed;
	public float FrogJumpingDelay;
	
	public float GetEggLayingDelay(Side side)
	{
		if (side == Side.Player)
		{
			return PlayerEggLayingDelay;
		}
		else
		{
			return EnemyEggLayingDelay;
		}
	}
}

