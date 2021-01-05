using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public float noiseLevel;
    public Material water;
    public Material grass;

    public void updateTile() {
        if(noiseLevel < 0.4) {
            transform.position = new Vector3(transform.position.x, -0.2f, transform.position.z);
            gameObject.GetComponent<Renderer>().material = water;
        } 
        else {
            gameObject.GetComponent<Renderer>().material = grass;
        }
    }
}
