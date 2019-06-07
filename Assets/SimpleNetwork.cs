using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimpleNetwork : MonoBehaviour {

	float[][][] net;
	float[][][] deltaNet;
	//int[] shape = new int[]{3, 5, 4, 5, 3};
	List<int> shape = new List<int>{3, 4, 4, 1};
	float[][] outputArray; //gives neuron values of every neuron
	//public GameObject testCube;
	float learningRate = 0.033f;
	List<float> testCorretOutput = new List<float>{0.8f, 0.3f, 0.5f};
	float[] testInput = new float[]{2, 0.3f, 1}; //must be length of the first value in shape list



	public void networkSetup (List<int> givenShape)
	{
		shape = givenShape;
		NewNetwork();
	}



	public float[] doCalculation (float[] inputArray)
	{
		for(int i = 0; i < inputArray.Length; i++){
			net[0][i][0] = inputArray[i];
		}

		FeedForward();

		float[] networkOutput = new float[outputArray[outputArray.Length - 1].Length];

		for(int i = 0; i < networkOutput.Length; i++){
			networkOutput[i] = outputArray[outputArray.Length - 1][i];
		}

		return networkOutput;
	}



	//!!!IMPORTANT!!!: The net is in terms net [y/i] [x/j] [z/k]. i = y, j = x, k = z 
	void NewNetwork () //the network, outputArray, and deltaNet are initialized here
	{
		//Debug.Log("Start");
		net = new float[shape.Count][][];
		deltaNet = new float[shape.Count][][];
		outputArray = new float[shape.Count][];
		for (int i = 0; i < shape.Count; i++) {
			net [i] = new float[shape [i]][];
			deltaNet [i] = new float[shape [i]][];
			outputArray [i] = new float[shape [i]];
			for (int j = 0; j < shape [i]; j++) {
				if (i != 0) {
					net [i] [j] = new float[(shape [i - 1]) + 1];
					deltaNet [i] [j] = new float[(shape [i - 1]) + 1];
				} else {
					net [i] [j] = new float[1];
					deltaNet [i] [j] = new float[1];
				}
				for (int k = 0; k < net [i] [j].Length; k++) {
					if (k != 0) {
						net [i] [j] [k] = UnityEngine.Random.Range (-0.3f, 0.3f); //initialize weights
					} else {
						net [i] [j] [0] = UnityEngine.Random.Range (-0.3f, 0.3f); //initialize biases 
					}
					//makes a visualization of the net array
					/*GameObject newCube;
					newCube = Instantiate (testCube, new Vector3 (j, -i, k), Quaternion.identity);
					newCube.name = string.Format("({0},{1},{2})", j, -i, k);
					newCube.transform.parent = transform;*/
				}
			}
		}
		//Debug.Log("Finish");
		/*Debug.Log(deltaArray.Length);
		for (int i = 0; i < outputArray.Length; i++){
			Debug.Log(deltaArray[i].Length);
		}*/
		//FeedForward();
	}


	//all weights for each neuron multiplyed by the sending neuron and added with all the other weights going to the reciveing neuron
	// and final sum added with the reciveing neuron value
	void FeedForward ()
	{
		//adding the inputs to the output array
		for (int i = 0; i < shape[0]; i++){
			outputArray[0][i] = net[0][i][0];
		}
		float temp = 0;

		//goes through each layer stating at the second layer (layer 1) because the first layer is only the input
		for (int i = 1; i < shape.Count; i++) {

			//goes through each neuron in the layer
			for (int j = 0; j < shape [i]; j++) {

				temp = 0;
				//goes through all the weights and neurons (biases) connecting to a neuron
				for (int k = 0; k < shape [i - 1]; k++) {
					if (i == 1) {
						temp += (net [0] [k] [0]) * (net [1] [j] [k + 1]); //k + 1 because the 0 layer is for the biases of the neurons also the first layer of the network is not biases but is the inputs
					} else {
						temp += outputArray [i - 1][k] * (net [i] [j] [k + 1]); //k + 1 because the 0 layer is for the biases of the neurons
					}
				}
				temp += net [i] [j] [0];
				temp = (float)Math.Tanh(temp); //hyperbolic tangent activation

				outputArray[i][j] = temp;
			}
		}

		//Getting outputs
		/*for (int i = 0; i < outputArray[outputArray.Length - 1].Length; i++){
			Debug.Log(outputArray[outputArray.Length - 1][i]);
		}*/

		//Get every propegated value
		/*for (int i = 0; i < outputArray.Length; i++){
			for (int j = 0; j < outputArray[i].Length; j++){
				Debug.Log(outputArray[i][j]);
			}
		}*/
		//BackPropegate(testCorretOutput);
	}


	public void BackPropagate (List<float> CorrectOutputList)
	{
		List<float> gammaList = new List<float> ();

		for (int i = shape.Count - 1; i > 0; i--) { //goes through every layer starting from the output layer. Its > 0 because input layer doesn't need backprop
			if (i != shape.Count - 1) {
				List<float> tempGammaList = new List<float>();

				for (int j = 0; j < shape[i]; j++){ //goes through each neuron in the current layer setting up gamma values
					float newGamma = 0;

					for (int k = 0; k < shape[i + 1]; k++){ //goes through each neuron in the previous layer setting up gamma values
						newGamma += gammaList[k] * net[i + 1][k][j + 1];
					}
					newGamma *= (1 - outputArray[i][j] * outputArray[i][j]); //derivative of the activation (tanh in this case)
					for (int k = 0; k <= shape [i - 1]; k++) { //goes through all weights and biases connecting to each neuron update deltaNet
						if (k != 0) {
							deltaNet[i][j][k] = learningRate * newGamma * outputArray[i - 1][k - 1]; //delta of weight
						} else {
							deltaNet[i][j][0] = learningRate * newGamma; // delta of bias
						}
					}
					tempGammaList.Add(newGamma);
				}
				gammaList.Clear();
				for (int j = 0; j < tempGammaList.Count; j++){ //making gammaList equal to tempGammaList
					gammaList.Add(tempGammaList[j]);
				}
			} else {
				for (int j = 0; j < shape [i]; j++) { //goes through each neuron in this layer (output layer) setting up gamma values
					float newGamma = 0;

					newGamma = (2f / CorrectOutputList.Count) * (outputArray [i] [j] - CorrectOutputList[j]) * (1 - (outputArray [i] [j] * outputArray [i] [j])); //using mean squared error as the cost function and tanh as the activation

					for (int k = 0; k <= shape [i - 1]; k++) { //goes through all weights and biases connecting to each neuron update deltaNet
						if (k != 0) {
							deltaNet[i][j][k] = learningRate * newGamma * outputArray[i - 1][k - 1]; //delta of weight
						} else {
							deltaNet[i][j][0] = learningRate * newGamma; //delta of bias
						}
					}
					gammaList.Add(newGamma);
				}
			}
		}
		for (int i = shape.Count - 1; i > 0; i--){ //update net by subtracting the deltaNet values. Its > 0 because input layer doesn't need backprop
			for (int j = 0; j < shape[i]; j++){
				for(int k = 0; k <= shape[i - 1]; k++){
					//Debug.Log(string.Format("{0}, {1}, {2}", i, j ,k));
					net[i][j][k] -= deltaNet[i][j][k];
					//Debug.Log(deltaNet[i][j][k]);
				}
			}
		}
		//Debug.Log("||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||");
	}
}
