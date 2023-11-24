using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeGrid : MonoBehaviour {

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	[SerializeField] private float nodeGizmosTransparency = 0.4f;

	public Transform testObj;
	
	[SerializeField] private GameObject markAsUnwalkable;
	private float minUnwalkableHeight = 0.005f;
	
	private string parentObjectName = "UNWALKABLE SPAWNER";

	
	void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x,y] = new Node(walkable,worldPoint, x,y);
			}
		}
	}

	public void CreateGridBasedOnVertices(MeshData meshData, Dictionary<TerrainType, Terrain> terrainByType, AnimationCurve heightCurve)
	{
		grid = new Node[meshData.meshWidth, meshData.meshHeight];
		
		nodeDiameter = nodeRadius * 2;
		gridSizeX = meshData.meshWidth;
		gridSizeY = meshData.meshHeight;
		
		// grid = new Node[gridSizeX, gridSizeY];

		/*
		for (int x = 0; x < gridSizeX; x++)
		{

			for (int y = 0; y < gridSizeY; y++)
			{
				{
					Vector3 vertex = meshData.vertices[y * meshData.meshWidth + x];
					// Debug.Log(vertex);
					float vertexHeight = vertex.y;
					// bool walkable = !Physics.CheckSphere(vertex, 10f, unwalkableMask);

					Collider[] objectCollisions = Physics.OverlapSphere(vertex, nodeRadius);
					bool walkable = true;
					foreach (var col in objectCollisions)
					{
						Debug.Log("Collider detected: " + col.name);
						// Only spawn if there is not an asset already near the location
						if (col.CompareTag("TerrainAsset"))
							walkable = false;
					}

					Debug.Log("walkable is " + walkable);
					grid[x, y] = new Node(walkable, vertex, x, y, vertexHeight);
				}
			}
			*/

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
				
				foreach (var t in terrainByType)
				{
					if (t.Value.type == TerrainType.Water && heightCurve.Evaluate(vertexHeight) < minUnwalkableHeight)
					{
						Debug.Log($"Compared evaluated h = {heightCurve.Evaluate(vertexHeight)} to terrain value height of {t.Value.height}");
						GameObject unwalkableObject = Instantiate(markAsUnwalkable, vertex * MapGeneration.meshScale, Quaternion.identity, unwalkableSpawner.transform);
						unwalkableObject.transform.localScale *= MapGeneration.meshScale;
						walkable = false;
					}
				}
				
				Collider[] objectCollisions = Physics.OverlapSphere(vertex, nodeRadius);
				foreach (var col in objectCollisions)
				{
					// Only spawn if there is not an asset already near the location
					if (col.CompareTag("TerrainAsset"))
						walkable = false;
				}

				Vector3 vertexPosition = new Vector3(vertex.x, vertex.y, vertex.z);
				// Vector3 vertexHeightRemoved = new Vector3(vertex.x, 1, vertex.z);
				grid[x, y] = new Node(walkable, vertexPosition, x, y, vertexHeight);
				// Debug.Log($"walkable ({walkable}) grid node created at [{x},{y}]");
			}

		}

		Node n = NodeFromWorldPoint(testObj.position);
		// Debug.Log($"TEST OBJ: walkable ({n.walkable}) grid node created at [{n.gridX},{n.gridY}] with world coords {n.worldPosition}");
		/*
		foreach (var v in meshData.vertices)
		{
			int x = (int)v.x;
			int y = (int)v.z;
			int vertexHeight = (int)v.y;

			Vector3 nodePosition = new Vector3(x, 0, y);

			Debug.Log(nodePosition);
			bool walkable = !Physics.CheckSphere(v, nodeRadius, unwalkableMask);
			grid[x, y] = new Node(walkable, nodePosition, x, y, vertexHeight);
		}
		*/

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

	public List<Node> path;
	void OnDrawGizmos() {
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