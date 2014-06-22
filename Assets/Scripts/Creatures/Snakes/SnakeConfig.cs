using System;
using UnityEngine;

public class SnakeConfig : MonoBehaviour
{
	public GameObject HeadPrefab;
	public GameObject BodyPrefab;
	public GameObject EggPrefab;
	public bool Player;
	public int BaseSpeed;
	public int SpeedPenaltyPerSegment;
	
	public UISprite BodySprite;
}


