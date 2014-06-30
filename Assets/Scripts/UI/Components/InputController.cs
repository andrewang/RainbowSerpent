using UnityEngine;
using Serpent;

public class InputController : MonoBehaviour
{
	public Snake PlayerSnake { get; set; }
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
		if (this.playerSnakeController == null)
		{
			this.playerSnakeController = this.PlayerSnake.Controller as PlayerSnakeController;
			if (this.playerSnakeController == null) { return; }
		}
		if (this.playerSnakeController.PlayerControlled == false) { return; }
		
		this.playerSnakeController.StartMoving(direction);
	}

	#endregion Input

}

