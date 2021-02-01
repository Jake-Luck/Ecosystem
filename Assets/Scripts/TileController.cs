using UnityEngine;

public class TileController : MonoBehaviour
{
    public float noiseLevel;
    public TerrainType[] regions;

    private Renderer tileRenderer;
    private BoxCollider boxCollider;

    public void Awake() {
        tileRenderer = GetComponent<Renderer>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void InitializeTile() {
        for (var i = 0; i < regions.Length; i++) {
            if (!(noiseLevel >= regions[i].colourLevel)) continue;
            
            tileRenderer.material.color = regions[i].colour;
            transform.position = new Vector3(transform.position.x, regions[i].height, transform.position.z);
        }

        if (!(noiseLevel < 0.5)) return;
        
        boxCollider.size = new Vector3(1, 5, 1);
        boxCollider.center = new Vector3(0, 2, 0);
    }

    public void updateTile() {
        noiseLevel += 0.05f;
        for (var i = 0; i < regions.Length; i++) {
            tileRenderer.material.color = regions[i].colour;
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
