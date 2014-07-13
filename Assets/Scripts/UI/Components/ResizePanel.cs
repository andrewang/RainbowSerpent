using System;
using UnityEngine;
using SerpentExtensions;

public class ResizePanel : MonoBehaviour
{
	private const int Top = 0;
	private const int Bottom = 1;
	private const int Left = 2;
	private const int Right = 3;
	private const int NumSides = 4;
	
	
	[SerializeField] UIWidget topWidget;
	[SerializeField] UIWidget bottomWidget;
	[SerializeField] UIWidget leftWidget;
	[SerializeField] UIWidget rightWidget;
	[SerializeField] GameObject parentObject;
	[SerializeField] bool vertical;
	[SerializeField] bool horizontal;
	[SerializeField] float borderSpace;
	
	private float[] positions;	
	private float[] dimensions;
	
	void Start()
	{
		this.positions = new float[NumSides];
		this.dimensions = new float[NumSides];
	}
	
	public ResizePanel ()
	{
	}
	
	// To ensure that this script is only executed once, but after any scale scripts:
	// execute in the Update and then disable itself
	void Update()
	{
		DeterminePositions();
		SetAtMidpoint();
		SizePanel();
		this.enabled = false;
	}
	
	private void DeterminePositions()
	{
		if (this.topWidget != null)
		{
			this.positions[Top] = this.topWidget.transform.GetLocalPositionRelativeTo(this.parentObject.transform).y;
			this.dimensions[Top] = this.topWidget.GetDimensionsRelativeTo(this.parentObject.transform).y;			
		}
		else
		{
			// use upper right corner of screen
			this.positions[Top] = Managers.ScreenManager.ScreenHeight * 0.5f;
			this.dimensions[Top] = 0f;
		} 
		
		if (this.bottomWidget != null)
		{
			this.positions[Bottom] = this.bottomWidget.transform.GetLocalPositionRelativeTo(this.parentObject.transform).y;
			this.dimensions[Bottom] = this.bottomWidget.GetDimensionsRelativeTo(this.parentObject.transform).y;		
		}
		else
		{
			// use lower left corner of screen
			this.positions[Bottom] = - Managers.ScreenManager.ScreenHeight * 0.5f;
			this.dimensions[Bottom] = 0f;		
		}
		
		if (this.leftWidget != null)
		{
			this.positions[Left] = this.leftWidget.transform.GetLocalPositionRelativeTo(this.parentObject.transform).x;
			this.dimensions[Left] = this.leftWidget.GetDimensionsRelativeTo(this.parentObject.transform).x;		
		}
		else
		{
			// use lower left corner of screen
			this.positions[Left] = - Managers.ScreenManager.ScreenWidth * 0.5f;
			this.dimensions[Left] = 0f;			
		}
		
		if (this.rightWidget != null)
		{
			this.positions[Right] = this.rightWidget.transform.GetLocalPositionRelativeTo(this.parentObject.transform).x;
			this.dimensions[Right] = this.rightWidget.GetDimensionsRelativeTo(this.parentObject.transform).x;		
		}
		else
		{
			this.positions[Right] = Managers.ScreenManager.ScreenWidth * 0.5f;
			this.dimensions[Right] = 0f;			
		}
	}
	
	void SetAtMidpoint()
	{
		Vector3 position = this.transform.localPosition;
			
		if (this.horizontal)
		{
			float x = Calculate1DMidPoint( this.positions[Left], this.dimensions[Left], this.positions[Right], this.dimensions[Right] );
			// truncate to whole pixel position
			x = Mathf.Floor(x);
			position.x = x;
		}
		if (this.vertical)
		{
			float y = Calculate1DMidPoint( this.positions[Top], this.dimensions[Top], this.positions[Bottom], this.dimensions[Bottom] );
			y = Mathf.Floor(y);
			position.y = y;
		}
		this.transform.localPosition = position;
	}
	
	void SizePanel()
	{
		UIPanel panel = this.gameObject.GetComponent<UIPanel>();
		if (panel == null) { return; }
				
		Vector4 clipRange = panel.clipRange;
		
		float horizontalScale = 1.0f;
		float verticalScale = 1.0f;
		
		if (this.horizontal && clipRange.z > 0f)
		{
			float dist = CalculateDistanceBetweenWidgets( this.positions[Left], this.dimensions[Left], this.positions[Right], this.dimensions[Right] );
			dist -= 2.0f * this.borderSpace;
			if (dist >= 0.0f)
			{
				horizontalScale = dist / clipRange.z;
			}
		}
		if (this.vertical && clipRange.w > 0f)
		{
			float dist = CalculateDistanceBetweenWidgets( this.positions[Top], this.dimensions[Top], this.positions[Bottom], this.dimensions[Bottom] );
			dist -= 2.0f * this.borderSpace;
			if (dist >= 0.0f)
			{
				verticalScale = dist / clipRange.w;
			}
		}

		float scalar = Mathf.Min(horizontalScale, verticalScale);
		// round to whole, even number pixels
		int z = (int) (clipRange.z * scalar);
		int w = (int) (clipRange.w * scalar);
		if (z % 2 == 1) { z -= 1; }
		if (w % 2 == 1) { w -= 1; }	
		clipRange.z = z;
		clipRange.w = w;
		panel.clipRange = clipRange;
	}
	
	float CalculateDistanceBetweenWidgets( float pos1, float width1, float pos2, float width2 )
	{
		if (pos1 > pos2)
		{
			return CalculateDistanceBetweenWidgets( pos2, width2, pos1, width1 );
		}
		
		float interiorPos1 = (pos1 + 0.5f * width1);
		float interiorPos2 = (pos2 - 0.5f * width2);
		
		return interiorPos2 - interiorPos1;
	}
	
	float Calculate1DMidPoint( float pos1, float width1, float pos2, float width2 )
	{
		if (pos1 > pos2)
		{
			return Calculate1DMidPoint( pos2, width2, pos1, width1 );
		}
		
		float interiorPos1 = (pos1 + 0.5f * width1);
		float interiorPos2 = (pos2 - 0.5f * width2);
		
		float distanceBetweenWidgets = interiorPos2 - interiorPos1;
		float halfDistance = distanceBetweenWidgets * 0.5f;
		return interiorPos1 + halfDistance;
	}
	
}

