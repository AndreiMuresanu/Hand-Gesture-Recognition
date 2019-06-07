using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GestureBatchData {

	public int[,,] gestureBatch = new int[500, 32, 24];

	public GestureBatchData (MasterControl mastercontrol)
	{
		//deep copy of gestureBatch
		int numberOfPics = mastercontrol.gestureBatch.GetLength(0);
		for(int i = 0; i < numberOfPics; i++){
			for(int j = 0; j < 32; j++){ //<32 because the image is 32 pixles wide
				for(int k = 0; k < 24; k++){ //<24 because the image is 24 pixels tall
					gestureBatch[i, j, k] = mastercontrol.gestureBatch[i, j, k];
				}
			}
		}
	}
}
