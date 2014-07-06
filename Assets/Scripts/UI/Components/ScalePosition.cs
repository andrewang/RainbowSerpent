using UnityEngine;

public class ScalePosition : MonoBehaviour
{
	#region Serialize Fields
	
	[SerializeField] private bool applyToHorizontal;
	[SerializeField] private bool applyToVertical;
	
	[SerializeField] private bool maintainProportionsScaledByWidth;
	[SerializeField] private bool maintainProportionsScaledByHeight;
	
	[SerializeField] private float baseWidth = 480;
	[SerializeField] private float baseHeight = 800;
	
	private float widthScale;
	private float heightScale;
	
	#endregion Serialize Fields
	
	void Start()
	{
		VerifySerializeFields();
		DetermineScales();
	}
	
	private void VerifySerializeFields()
	{
		if (this.applyToHorizontal == false && this.applyToVertical == false)
		{
			Debug.LogError("ScalePosition applies neither to horizontal or vertical");
		}
		
		if (this.maintainProportionsScaledByWidth && this.maintainProportionsScaledByHeight)    
		{
			Debug.LogError("ScalePosition has maintain propertions set, and BOTH scale by width and scale by height are set");
		}
	}
	
	private void DetermineScales()
	{
		this.widthScale = Screen.width / this.baseWidth;
		this.heightScale = Screen.height / this.baseHeight;
		
		if (this.maintainProportionsScaledByWidth)
		{
			this.heightScale = this.widthScale;
		}
		else if (this.maintainProportionsScaledByHeight)
		{
			this.widthScale = this.heightScale;
		}
		
		Vector3 position = this.transform.localPosition;
		if (this.applyToHorizontal)		
		{
			position.x *= this.widthScale;
		}
		if (this.applyToVertical)
		{
			position.y *= this.heightScale;
		}
		
		this.transform.localPosition = position;
	}
	
}


