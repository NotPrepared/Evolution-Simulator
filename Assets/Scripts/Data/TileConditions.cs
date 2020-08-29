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
		//Debug.Log("Height Value in Tile Condition:" + height.ToString());
		bool valid = (height > minHeight) && (height <= maxHeight);
		//Debug.Log("Valid Bool: " + valid.ToString());
		return valid;
	}
	
	

}
