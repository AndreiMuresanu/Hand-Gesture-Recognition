using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MasterControl : MonoBehaviour {
	WebCamTexture wct; //camera being used it 160 by 120 pixels

	public RawImage Camera;
	public RawImage Calibration;
	public RawImage ColourCut;
	public RawImage Filtered;

	float lowH = 0f;
	float highH = 1f;
	float lowS = 1f;
	float highS = 0f;
	float lowV = 1f;
	float highV = 0f;

	float squareSize = 0.25f; //% of image that will be used for calibration (hand needs to be contained within the calibration box so the HSV limits can be found)

	Texture2D reducedImage;

	public Text recordingText;
	bool recording = false;
	int counter = 0;

	//the saving process would probably work better (far more reliable) with a list of batches, but using gestureQuantity makes it easier
	public int[,,] gestureBatch = new int[500, 32, 24]; //[number of images, width of image, height of image], throughout the project it is assumed that these will not change (just to make stuff easier)
	//using int instead of bool because I might switch to greyscale in the future
	public int gestureQuantity; //total number of gestures that the network can recognize


	public SimpleNetwork network;
	List<int> networkShape = new List<int>{768, 512, 256, 128, 4}; //network will only recognize 4 gestures for now
	List<List<int[,]>> MEGAgestureBatch = new List<List<int[,]>>(); //contains all gesture images (used in training), assumes there are only 4 gestures
	float[] networkOutput;

	bool isdetecting = false;
	public Text isdetectingtext;
	public Text detectionResult;



	//IMPORTANT: for the network to work properly the calibration needs to be done properly.
	//The black and white image should only contain pixels from the hand and there should be minimal gaps inside the hand.
	//The network needs to have the same conditions it had during training for it to work.


	// Use this for initialization
	void Start ()
	{
		//loads gestureQuantity on startup to maintian continuity
		string path = "C:/Users/Andrei/Documents/Unity Games/Hand Gesture Recognition/GestureAlbums/_gestureQuantity.banana";
		if (File.Exists (path)) {
			LoadGestureQuantity ();
		} else {
			resetGestureQuantity ();
		}
		Debug.Log ("gestureQuantity: " + gestureQuantity);


		WebCamDevice[] devices = WebCamTexture.devices;
		wct = new WebCamTexture (WebCamTexture.devices [0].name, 160, 120, 12); //width and height values don't matter, I think

		Camera.texture = wct;

		wct.Play ();


		//for testing
		/*LoadGestureBatch (0);
		reducedImage = new Texture2D (32, 24);
		for (int x = 0; x < 32; x++) {
			for (int y = 0; y < 24; y++) {
				if (gestureBatch [301, x, y] == 1) {
					reducedImage.SetPixel (x, y, Color.white);
				} else {
					reducedImage.SetPixel (x, y, Color.black);
				}
			}
		}
		reducedImage.Apply();
        Filtered.texture = reducedImage;*/


	}


	// Update is called once per frame
	void Update ()
	{
		if (recording == true) {
			recordingText.text = "RECORDING";

			if((counter + 1) % 50 == 0){
				Debug.Log("Pictures Taken: " + (counter + 1));
			}

			TakePic();

			if (counter == 499) { //number of images that are going to be created
				counter = 0;
				recording = false;
				SaveGestureBatch();
			} else {
				counter += 1;
			}
		} else {
			recordingText.text = "Not Recording";
		}

		if(isdetecting == true){
			TakePic();
		}
	}


	public void Calibrate (){ //calibration works best if you make a first with your hand and have your hand completely contained within the yellow box
		Texture2D snap = new Texture2D(wct.width, wct.height);
        snap.SetPixels(wct.GetPixels());


		highH = 1;
		lowH = 0;
		lowS = 1f;
		highS = 0f;
		lowV = 1f;
		highV = 0f;


		for (int y = Mathf.RoundToInt(((float)(wct.height / 2) * (1 - squareSize))); y < wct.height - Mathf.RoundToInt(((float)(wct.height / 2) * (1 - squareSize))); y++)
		{ //the sizes are a rearangement
			for (int x = Mathf.RoundToInt(((float)(wct.width / 2) * (1 - squareSize))); x < wct.width - Mathf.RoundToInt(((float)(wct.width / 2) * (1 - squareSize))); x++)
            {
				Color PixelColor = snap.GetPixel(x, y);
				float pixH, pixS, pixV;

				Color.RGBToHSV (PixelColor, out pixH, out pixS, out pixV);

				if(pixH > 0.5f){
					if(pixH < highH){
						highH = pixH;
					}
				} else if (pixH > lowH){
					lowH = pixH;
				}

				if(pixS > highS){
					highS = pixS;
				} else if (pixS < lowS){
					lowS = pixS;
				}

				if(pixV > highV){
					highV = pixV;
				} else if (pixV < lowV){
					lowV = pixV;
				}


				snap.SetPixel(x, y, Color.yellow);
            }
        }
		snap.Apply();
		Calibration.texture = snap;
		//Debug.Log("High Limit: " + highH + "   S: " + highS + "   V: " + highV);
		//Debug.Log("Low  Limit: " + lowH + "   S: " + lowS + "   V: " + lowV);

	}


	public void TakePic(){
		Texture2D snap = new Texture2D(wct.width, wct.height);
        snap.SetPixels(wct.GetPixels());


		for (int y = 0; y < snap.height; y++)
        {
			for (int x = 0; x < snap.width; x++)
            {
				Color PixelColor = snap.GetPixel(x, y);
				float pixH, pixS, pixV;

				Color.RGBToHSV (PixelColor, out pixH, out pixS, out pixV);


				if((pixH <= lowH || pixH >= highH) && (pixS >= lowS && pixS <= highS) && (pixV >= lowV && pixV <= highV)){
					snap.SetPixel(x, y, Color.white);
				} else{
					snap.SetPixel(x, y, Color.black);
				}
            }
        }

		snap.Apply();
		ColourCut.texture = snap;
		sizeReduction(snap);
	}


	public void sizeReduction (Texture2D colourCutImage)
	{
		Texture2D snap = new Texture2D (colourCutImage.width, colourCutImage.height);
		snap.SetPixels (colourCutImage.GetPixels ());

		reducedImage = new Texture2D (32, 24);


		//turning the hsv filtered image into a 32 by 24 image (number were chosen to minimize nn inputs and because they fit nicely with the camera I use)
		//for cameras of input sizes that are not multiples of 32 and 24, the excess remaining pixels are cut from the top and right of the image 
		for (int y = 0; y < 24; y++) {
			for (int x = 0; x < 32; x++) {

				int regionAverage = 0;

				//loop through each pixel in the 24 by 32 block to create an average and set pixel to either black or white 
				//(average instead of simply counting pixels since in the future greyscale might be used)
				for (int i = ((snap.height - snap.height % 24) / 24) * y; i < ((snap.height - snap.height % 24) / 24) * (y + 1); i++) {
					for (int j = ((snap.width - snap.width % 32) / 32) * x; j < ((snap.width - snap.width % 32) / 32) * (x + 1); j++) {

						if (snap.GetPixel (j, i) == Color.white) {
							regionAverage += 1;
						}
					}
				}

				if (regionAverage >= (((snap.height - snap.height % 24) / 24) * ((snap.width - snap.width % 32) / 32)) / 2) {
					reducedImage.SetPixel (x, y, Color.white);
					if (recording == true) {
						gestureBatch [counter, x, y] = 1;
					}
				} else {
					reducedImage.SetPixel (x, y, Color.black);
					if (recording == true) {
						gestureBatch [counter, x, y] = 0;
					}
				}
			}
		}

		snap.Apply ();
		reducedImage.Apply ();
		Filtered.texture = reducedImage;

		if (isdetecting == true) {
			float[] networkInput = new float[768]; //24 * 32 = 768

			for (int x = 0; x < 32; x++) {
				for (int y = 0; y < 24; y++) {
					if (reducedImage.GetPixel (x, y) == Color.white) {
						networkInput [24 * x + y] = 1;
					} else {
						networkInput [24 * x + y] = 0;
					}
				}
			}

			networkOutput = new float[4]; //assumes only 4 gestures
			networkOutput = network.doCalculation (networkInput);

			if (networkOutput [0] > networkOutput [1] && networkOutput [0] > networkOutput [2] && networkOutput [0] > networkOutput [3]) {
				detectionResult.text = "Open Hand";
			} else if (networkOutput [1] > networkOutput [0] && networkOutput [1] > networkOutput [2] && networkOutput [1] > networkOutput [3]) {
				detectionResult.text = "Peace";
			} else if (networkOutput [2] > networkOutput [0] && networkOutput [2] > networkOutput [1] && networkOutput [0] > networkOutput [3]) {
				detectionResult.text = "Rock";
			} else {
				detectionResult.text = "Fist";
			}
       	}
	}


	//sets gesture quantity to 0 and saves it. 
	public void resetGestureQuantity ()
	{
		gestureQuantity = 0;
		GestureSaveSystem.SaveGestureQuantity(this);
	}


	public void SaveGestureQuantity ()
	{
		GestureSaveSystem.SaveGestureQuantity(this);
		Debug.Log("gestureQuantity SAVED, value: " + gestureQuantity);
	}


	public void LoadGestureQuantity ()
	{
		GestureQuantityData data = GestureSaveSystem.LoadGestureQuantity();

		gestureQuantity = data.gestureQuantity;
	}


	public void SaveGestureBatch ()
	{
		GestureSaveSystem.SaveGestureBatch(this, gestureQuantity - 1);
		Debug.Log("gestureBatch SAVED, number: " + gestureQuantity);
	}


	public void LoadGestureBatch (int gestureNumber)
	{
		GestureBatchData data = GestureSaveSystem.LoadGestureBatch(gestureNumber);

		//deep copy of gestureBatch
		for(int i = 0; i < 500; i++){
			for(int j = 0; j < 32; j++){ //<32 because the image is 32 pixles wide
				for(int k = 0; k < 24; k++){ //<24 because the image is 24 pixels tall
					gestureBatch[i, j, k] = data.gestureBatch[i, j, k];
				}
			}
		}
	}



	public void LoadMEGAgestureBatch ()
	{
		MEGAgestureBatch.Clear();

		for (int n = 0; n < 4; n++){ //assumes only 4 gestures are being used

			List<int[,]> imagecollection = new List<int[,]>();

			GestureBatchData data = GestureSaveSystem.LoadGestureBatch(n);

			//deep copy of gestureBatch
			for(int i = 0; i < 500; i++){
				int[,] gestureImage = new int[32, 24];
				for(int j = 0; j < 32; j++){ //<32 because the image is 32 pixles wide
					for(int k = 0; k < 24; k++){ //<24 because the image is 24 pixels tall
						gestureImage[j, k] = data.gestureBatch[i, j, k];
					}
				}
				imagecollection.Add(gestureImage);
			}

			MEGAgestureBatch.Add(imagecollection);

			//for testing
			/*if(n == 1){
				reducedImage = new Texture2D (32, 24);
				for (int x = 0; x < 32; x++) {
					for (int y = 0; y < 24; y++) {
						if (MEGAgestureBatch[0][304][x, y] == 1) {
							reducedImage.SetPixel (x, y, Color.white);
						} else {
							reducedImage.SetPixel (x, y, Color.black);
						}
					}
				}
				reducedImage.Apply();
		        Filtered.texture = reducedImage;
	        }*/
		}
	}


	//Records camera to get images and then creates a new gesture folders and saves them there
	public void newGesture(){
		if(recording == false){
			recording = true;
			gestureQuantity += 1;
			SaveGestureQuantity();
		}
	}


	//Instead of adding a new gesture to the network it completely retrains the network
	//this can be avoided by copying all the old weights into the new network and only training the weights connecting to the output, but thats a future project 
	//ajustable ajusting learning rate, batches, and dropout are being used out of simiplicity
	//Training also assumes 4 gestures, this can be easily modified though
	public void trainNetwork ()
	{
		//create a new network
		network.networkSetup (networkShape);

		//retrieve all gesture images
		LoadMEGAgestureBatch ();

		//train the network
		for (int i = 0; i < 500; i++) { //500 images
			for (int j = 0; j < 4; j++) { //assumes only 4 gestures
				
				int randomIndex = Random.Range (0, MEGAgestureBatch [j].Count); //min is inclusive, max is exclusive

				float[] networkInput = new float[768]; //24 * 32 = 768
				for (int x = 0; x < 32; x++) {
					for (int y = 0; y < 24; y++) {
						networkInput [24 * x + y] = MEGAgestureBatch [j] [randomIndex] [x, y];
					}
				}
				networkOutput = new float[4]; //assumes only 4 gestures
				networkOutput = network.doCalculation (networkInput);

				if (i == 499 || i == 488 || i == 477) {
					Debug.Log ("gesture: " + j + " output: " + networkOutput [0]+ ":   " + networkOutput [1]+ ":   " + networkOutput [2]+ ":   " + networkOutput [3]);
				}

				List<float> correctOutput = new List<float> ();
				correctOutput.Clear();

				if (j == 0) {
					correctOutput.Add (1);
					correctOutput.Add (0);
					correctOutput.Add (0);
					correctOutput.Add (0);
				} else if (j == 1) {
					correctOutput.Add (0);
					correctOutput.Add (1);
					correctOutput.Add (0);
					correctOutput.Add (0);
				} else if (j == 2) {
					correctOutput.Add (0);
					correctOutput.Add (0);
					correctOutput.Add (1);
					correctOutput.Add (0);
				} else {
					correctOutput.Add (0);
					correctOutput.Add (0);
					correctOutput.Add (0);
					correctOutput.Add (1);
				}

				network.BackPropagate(correctOutput);

				MEGAgestureBatch[j].RemoveAt(randomIndex);
			}
		}

	}


	public void startDetecting ()
	{
		isdetecting = !isdetecting;
		if (isdetecting == false) {
			isdetectingtext.text = "Not Detecting";
		} else {
			isdetectingtext.text = "DETECTING";
		}
	}
}
