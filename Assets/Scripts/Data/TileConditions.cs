using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TileConditions : ScriptableObject {

	public float minHeight;
	public float maxHeight;
	public float minHumidity;
	public float maxHumidity;
	public float minTemperature;
	public float maxTemperature;
	
	public GameObject prefabTile;
	
	public bool CheckIfValid(float height, float humidity, float temperature)
	//, float humidity, float temperature
	{
		Debug.Log("Height Value in Tile Condition:" + height.ToString());
		Debug.Log("Humidity Value in Tile Condition:" + humidity.ToString());
		Debug.Log("Temperature Value in Tile Condition:" + temperature.ToString());

		bool validHeight = height > minHeight && height <= maxHeight;
		bool validHumidity = (humidity > minHumidity) && (humidity <= maxHumidity);
		bool validTemperature = (temperature > minTemperature) && (temperature <= maxTemperature);
		//Debug.Log("Valid Bool: " + valid.ToString());
		return validHeight&&validHumidity&&validTemperature;
	}
	
	

}
