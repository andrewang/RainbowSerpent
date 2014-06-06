using UnityEngine;

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
		OnPressDirection(SerpentConsts.Dir.N);
	}
	
	private void OnPressDown()
	{
		OnPressDirection(SerpentConsts.Dir.S);
	}
	
	private void OnPressLeft()
	{
		OnPressDirection(SerpentConsts.Dir.W);
	}
	
	private void OnPressRight()
	{
		OnPressDirection(SerpentConsts.Dir.E);
	}
	
	private void OnPressDirection(SerpentConsts.Dir direction)
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

