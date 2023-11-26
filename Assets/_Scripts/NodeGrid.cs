using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NodeGrid : MonoBehaviour {

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	[SerializeField] private float nodeGizmosTransparency = 0.4f;

	public Transform testObj;
	
	[SerializeField] private GameObject markAsUnwalkable;
	private float minUnwalkableHeight = 0.005f;
	
	private string parentObjectName = "UNWALKABLE SPAWNER";

	public List<Node> path = new List<Node>();

	public List<NodeGrid> allOtherNodeGrids;

	private Node previousNpcAtNode;

	private void Awake()
	{

	}

	public void AddOtherNodeGrids()
	{
		allOtherNodeGrids = Resources.FindObjectsOfTypeAll<NodeGrid>().ToList();
		
		foreach (var nodeGrid in allOtherNodeGrids.ToList())
		{
			if (nodeGrid == this)
			{
				allOtherNodeGrids.Remove(nodeGrid);
			}
		}
	}

	private void Update()
	{
		Node npcAtNode = NodeFromWorldPoint(transform.position);

		if (previousNpcAtNode != null && previousNpcAtNode.isAsset == false)
		{
			ChangeNodeWalkable(previousNpcAtNode, true);

		}

		ChangeNodeWalkable(npcAtNode, false);

		previousNpcAtNode = npcAtNode;
	}

	private void ChangeNodeWalkable(Node n, bool walkable)
	{
		foreach (var nodeGrid in allOtherNodeGrids.ToList())
		{
			nodeGrid.grid[n.gridX, n.gridY].walkable = walkable;
		}
	}

	public void CreateGridBasedOnVertices(MeshData meshData, Dictionary<TerrainType, Terrain> terrainByType, AnimationCurve heightCurve)
	{
		grid = new Node[meshData.meshWidth, meshData.meshHeight];
		
		nodeDiameter = nodeRadius * 2;
		gridSizeX = meshData.meshWidth;
		gridSizeY = meshData.meshHeight;
		
		GameObject spawnedUnwalkableObjects = GameObject.Find(parentObjectName);
		if (spawnedUnwalkableObjects != null)
		{
			DestroyImmediate(spawnedUnwalkableObjects);
		}
		
		GameObject unwalkableSpawner = new GameObject(parentObjectName);

		
		for (int y = 0; y < gridSizeY; y++)
		{
			for (int x = 0; x < gridSizeX; x++)
			{
				Vector3 vertex = meshData.vertices[y * gridSizeX + x];
				float vertexHeight = vertex.y;
				bool walkable = true;
				bool isAsset = false;
				int movePenalty = 0;
				
				foreach (var t in terrainByType)
				{
					if (t.Value.type == TerrainType.Water && heightCurve.Evaluate(vertexHeight) < minUnwalkableHeight)
					{
						// Debug.Log($"Compared evaluated h = {heightCurve.Evaluate(vertexHeight)} to terrain value height of {t.Value.height}");
						GameObject unwalkableObject = Instantiate(markAsUnwalkable, vertex * MapGeneration.meshScale, Quaternion.identity, unwalkableSpawner.transform);
						unwalkableObject.transform.localScale *= MapGeneration.meshScale;
						walkable = false;
						isAsset = true;
					}
					
					switch (t.Value.type)
					{
						case TerrainType.Sand:
							movePenalty = 2;
							break;
						case TerrainType.LightGrass:
							movePenalty = 1;
							break;
						case TerrainType.DarkGrass:
							movePenalty = 3;
							break;
						case TerrainType.LightRock:
							movePenalty = 5;
							break;
						case TerrainType.DarkRock:
							movePenalty = 10;
							break;
						case TerrainType.Snow:
							movePenalty = 20;
							break;
					}
				}
				
				Collider[] objectCollisions = Physics.OverlapSphere(vertex, nodeRadius);
				foreach (var col in objectCollisions)
				{
					// Mark node as unwalkable if there is an asset on it
					if (col.CompareTag("TerrainAsset"))
					{
						walkable = false;
						isAsset = true;
					}
				}

				Vector3 vertexPosition = new Vector3(vertex.x, vertex.y, vertex.z);
				grid[x, y] = new Node(walkable, isAsset, vertexPosition, x, y, movePenalty, vertexHeight);
				// Debug.Log($"walkable: ({walkable}) grid node created at: [{x},{y}]");
			}

		}

		// Node n = NodeFromWorldPoint(testObj.position);
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		worldPosition.z *= -1; // Fix incorrect sign
		
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		// Debug.Log("nodepoint: " + x + " : " + y);
		return grid[x,y];
	}
	
	void OnDrawGizmosSelected() {
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y));

		if (grid != null) {
			Color walkableColour = new Color(Color.white.r, Color.white.g, Color.white.b, nodeGizmosTransparency);
			Color notWalkableColour = new Color(Color.red.r, Color.red.g, Color.red.b, nodeGizmosTransparency);
			Color pathColour = new Color(Color.black.r, Color.black.g, Color.black.b, nodeGizmosTransparency);
			foreach (Node n in grid)
			{
				Gizmos.color = n.walkable ? walkableColour : notWalkableColour;
				
				if (path != null)
					if (path.Contains(n))
					{
						Gizmos.color = pathColour;
						// Debug.Log($"path drawn at [{n.gridX},{n.gridY}] with world coords {n.worldPosition}");
					}
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}
}