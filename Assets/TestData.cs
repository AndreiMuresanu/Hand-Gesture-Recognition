using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestData {

	public float number;


	public TestData (TestControl testcontrol)
	{
		number = testcontrol.number;
	}

}
