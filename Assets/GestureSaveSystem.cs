using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class GestureSaveSystem {
	
	public static void SaveGestureQuantity (MasterControl mastercontrol)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/GestureAlbums/_gestureQuantity.banana";
		FileStream stream = new FileStream(path, FileMode.Create);

		GestureQuantityData data = new GestureQuantityData(mastercontrol);

		formatter.Serialize(stream, data);
		stream.Close();
	}

	public static GestureQuantityData LoadGestureQuantity ()
	{
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/GestureAlbums/_gestureQuantity.banana";
		if(File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			GestureQuantityData data = formatter.Deserialize(stream) as GestureQuantityData;
			stream.Close();

			return data;

		} else 
		{
			Debug.LogError("Save file not found in " + path);
			return null;
		}
	}




	public static void SaveGestureBatch (MasterControl mastercontrol, int gestureNumber)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/GestureAlbums/" + gestureNumber + "gestureBatch.banana";
		FileStream stream = new FileStream(path, FileMode.Create);

		GestureBatchData data = new GestureBatchData(mastercontrol);

		formatter.Serialize(stream, data);
		stream.Close();
	}

	public static GestureBatchData LoadGestureBatch (int gestureNumber)
	{
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/GestureAlbums/" + gestureNumber + "gestureBatch.banana";
		if(File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			GestureBatchData data = formatter.Deserialize(stream) as GestureBatchData;
			stream.Close();

			return data;

		} else 
		{
			Debug.LogError("Save file not found in " + path);
			return null;
		}
	}
}
