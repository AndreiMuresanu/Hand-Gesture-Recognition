using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class TestSaveSystem {

	public static void SaveTestControl (TestControl testcontrol)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/SaveTestFolder/testcontrol.banana";
		FileStream stream = new FileStream(path, FileMode.Create);

		TestData data = new TestData(testcontrol);

		formatter.Serialize(stream, data);
		stream.Close();
	}

	public static TestData LoadTestControl ()
	{
		//string path = Application.persistentDataPath + "/testcontrol.banana";
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/SaveTestFolder/testcontrol.banana";
		if(File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(path, FileMode.Open);

			TestData data = formatter.Deserialize(stream) as TestData;
			stream.Close();

			return data;

		} else 
		{
			Debug.LogError("Save file not found in " + path);
			return null;
		}
	}
}
