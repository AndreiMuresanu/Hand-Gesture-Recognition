using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GestureQuantityData {

	public int gestureQuantity;

	public GestureQuantityData (MasterControl mastercontrol)
	{
		gestureQuantity = mastercontrol.gestureQuantity;
	}
}
