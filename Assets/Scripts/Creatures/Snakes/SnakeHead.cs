using UnityEngine;
using System.Collections;
using Serpent;

public class SnakeHead : SnakeSegment
{		
	[SerializeField] private UISpriteAnimation biteAnimation;
	
	private InteractionState interactionState = InteractionState.Nothing;
	
	/// <summary>
	/// Updates the position of the head
	/// </summary>
	/// <returns><c>true</c>, if head arrived at its destionation, <c>false</c> otherwise.</returns>
	/// <param name="displacement">Displacement.</param>
	/// <param name="remainingDisplacement">Remaining displacement.</param>
	public bool MoveForward(float displacement, out float remainingDisplacement)
	{
		Vector3 toDest = this.CurrentDestination - this.transform.localPosition;		
		Vector3 nextPos = this.transform.localPosition + (this.CurrentDirectionVector * displacement);
		Vector3 afterMoveToDest = this.CurrentDestination - nextPos;
		
		if (Vector3.Dot(toDest, afterMoveToDest) > 0)
		{
			// Have not reached current destionation so just move.
			this.transform.localPosition = nextPos;
			
			// Did not arrive at destination
			remainingDisplacement = 0.0f;
			return false;
		}
					
		// Update the head position to exactly be the destination
		this.transform.localPosition = this.CurrentDestination;
		
		// Return how much movement wasn't used.		
		remainingDisplacement = displacement - toDest.magnitude;
		return true;
	}	
	
	public void PlayTongueAnimation()
	{
		// set namePrefix on animation
		if (this.biteAnimation == null) { return; }
		
		this.biteAnimation.namePrefix = "Tongue";
	}
	
	public void PlayBiteAnimation()
	{
		// set namePrefix on animation
		if (this.biteAnimation == null) { return; }

		this.biteAnimation.namePrefix = "Bite";
	}
	
	public void UpdateInteractionState(InteractionState interactionState)
	{
		if (interactionState == this.interactionState) 
		{
			return;
		}		
		this.interactionState = interactionState;
		
		if (interactionState == InteractionState.Nothing)
		{
			PlayTongueAnimation();
		}
		else
		{
			PlayBiteAnimation();
		}
	}
}
