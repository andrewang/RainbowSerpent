using UnityEngine;
using System.Reflection;
using System;

public interface CyclicSettingsDataSource
{
	void SetValue(string valueName, int value);
	int GetValue(string valueName);
}

public class CyclicSettingsLabel : MonoBehaviour
{
	// This label will 
	[SerializeField] UILabel label;
	[SerializeField] GameObject target;
	[SerializeField] string dataSourceScriptName;
	[SerializeField] string valueName;
	[SerializeField] string[] displayValues;
	
	CyclicSettingsDataSource dataSource;
	int currentValue;
	
	private void Start()
	{
		VerifySerializeFields();
		
		if (this.target == null) 
		{
			return;
		}
		Component component = this.target.GetComponent(this.dataSourceScriptName);
		this.dataSource = component as CyclicSettingsDataSource;
		
		if (this.label == null)
		{
			this.label = this.gameObject.GetComponent<UILabel>();
		}
		
		this.currentValue = this.dataSource.GetValue(this.valueName);
		
		UpdateLabel();
		
	}
	
	public void VerifySerializeFields()
	{
		if (this.target == null) { Debug.LogError("CyclicSettingsLabel: no target"); }
	}
	
	public void ChangeToNextValue()
	{
		this.currentValue = (this.currentValue + 1) % this.displayValues.Length;
		this.dataSource.SetValue(this.valueName, this.currentValue);		
		
		UpdateLabel();
	}
	
	private void UpdateLabel()
	{
		if (this.currentValue >= this.displayValues.Length)
		{
			return;
		}
		string displayValue = this.displayValues[this.currentValue];
		this.label.text = displayValue;
	}
}

