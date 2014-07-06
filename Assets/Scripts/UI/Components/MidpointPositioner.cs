using System;
using UnityEngine;
using SerpentExtensions;

public class MidpointPositioner : MonoBehaviour
{
		
	[SerializeField] UIWidget widget1;
	[SerializeField] UIWidget widget2;
	[SerializeField] GameObject parentObject;
	[SerializeField] bool vertical;
	[SerializeField] bool horizontal;
	
	public MidpointPositioner ()
	{
	}
	
	// To ensure that this script is only executed once, but after any scale scripts:
	// execute in the Update and then disable itself
	void Update()
	{
		SetAtMidpoint(this.widget1, this.widget2);
		this.enabled = false;
	}
	
	void SetAtMidpoint( UIWidget widget1, UIWidget widget2 )
	{
		// Calculate the mid point of the two widgets based on their local positions, scale, and dimensions
		Vector3 position1 = widget1.transform.GetLocalPositionRelativeTo(this.parentObject.transform);
		Vector3 dimensions1 = widget1.GetDimensionsRelativeTo(this.parentObject.transform);
		
		Vector3 position2 = widget2.transform.GetLocalPositionRelativeTo(this.parentObject.transform);
		Vector3 dimensions2 = widget2.GetDimensionsRelativeTo(this.parentObject.transform);
				
		Vector3 position = this.transform.localPosition;
		if (this.horizontal)
		{
			float x = Calculate1DMidPoint( position1.x, dimensions1.x, position2.x, dimensions2.x );
			position.x = x;
		}
		if (this.vertical)
		{
			float y = Calculate1DMidPoint( position1.y, dimensions1.y, position2.y, dimensions2.y );
			position.y = y;
		}
		this.transform.localPosition = position;
	}
	
	float Calculate1DMidPoint( float pos1, float width1, float pos2, float width2 )
	{
		if (pos1 > pos2)
		{
			return Calculate1DMidPoint( pos2, width2, pos1, pos1 );
		}
		
		float interiorPos1 = (pos1 + 0.5f * width1);
		float interiorPos2 = (pos2 - 0.5f * width2);
		
		float distance = interiorPos2 - interiorPos1;
		float halfDistance = distance * 0.5f;
		return interiorPos1 + halfDistance;
	}
	
	
}

