using UnityEngine;
using System.Collections;

[CreateAssetMenu()]
public class TileConditions : ScriptableObject {

	public float minHeight;
	public float maxHeight;

	public GameObject prefabTile;
	
	public bool CheckIfValid(float height)
	{
		Debug.Log("Height Value in Tile Condition:" + height.ToString());
		//if (height == 0) height;
		bool valid = (height > minHeight) && (height <= maxHeight);
		Debug.Log("Valid Bool: " + valid.ToString());
		return valid;
	}
	
	

}
