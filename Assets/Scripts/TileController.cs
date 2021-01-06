using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public float noiseLevel;
    public Renderer tileRenderer;

    public TerrainType[] regions;

    public void Awake() {
        tileRenderer = GetComponent<Renderer>();
    }

    public void updateTile() {
        for (int i = 0; i < regions.Length; i++) {
            if (noiseLevel >= regions[i].colourLevel) {
                tileRenderer.material.color = regions[i].colour;
                transform.position = new Vector3(transform.position.x, regions[i].height, transform.position.z);
            }
        }
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float colourLevel;
    public Color colour;
    public float height;
}
