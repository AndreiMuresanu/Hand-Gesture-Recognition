using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestControl : MonoBehaviour {

	public Text numberDisplay;
	public float number;


	void Start(){
		number = Random.Range(0.0f, 10f);
		numberDisplay.text = number.ToString();
	}


	public void add ()
	{
		number += 1;
		numberDisplay.text = number.ToString();
	}


	public void subtract ()
	{
		number -= 1;
		numberDisplay.text = number.ToString();
	}


	public void SaveControl ()
	{
		TestSaveSystem.SaveTestControl(this);
	}


	public void LoadControl ()
	{
		TestData data = TestSaveSystem.LoadTestControl();

		number = data.number;

		numberDisplay.text = number.ToString();
	}
}
