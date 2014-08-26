using UnityEngine;
using Serpent;

public class InputController : MonoBehaviour
{
	private Snake playerSnake;
	public Snake PlayerSnake
	{
		get
		{
			return playerSnake;
		}
		set
		{
			this.playerSnake = value;
			this.playerSnakeController = this.playerSnake.Controller as PlayerSnakeController;
		}
	}
	private PlayerSnakeController playerSnakeController;
	
	public InputController ()
	{
	}

	#region Input
	
	private void OnPressUp()
	{
		OnPressDirection(Direction.N);
	}
	
	private void OnPressDown()
	{
		OnPressDirection(Direction.S);
	}
	
	private void OnPressLeft()
	{
		OnPressDirection(Direction.W);
	}
	
	private void OnPressRight()
	{
		OnPressDirection(Direction.E);
	}
	
	private void OnPressDirection(Direction direction)
	{
		if (this.PlayerSnake.Dead) { return; }
		if (this.playerSnakeController.PlayerControlled == false) 
		{ 
			this.playerSnakeController.SetDesiredDirection(direction);
			return; 
		}
		
		this.playerSnakeController.StartMoving(direction);
	}

	#endregion Input

}

