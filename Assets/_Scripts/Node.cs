using UnityEngine;
using System.Collections;

public class Node {
	
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost; // Distance from the starting node
    public int hCost; // Distance from the end node
    public Node parent;

    public float meshVertexHeightAtNode;
	
    public Node(bool walkable, Vector3 worldPos, int gridX, int gridY, float meshVertexHeightAtNode = 1f) {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
        this.meshVertexHeightAtNode = meshVertexHeightAtNode;
    }

    // F cost = G cost + H cost
    public int fCost {
        get {
            return gCost + hCost;
        }
    }
}