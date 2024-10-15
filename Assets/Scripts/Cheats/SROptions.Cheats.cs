using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
	[Category("Data")]
	public void ResetAllData()
	{
		PlayerPrefs.DeleteAll();
	}
}