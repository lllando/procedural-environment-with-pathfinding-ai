using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGeneration))]
public class MapGenerationEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        MapGeneration mapGeneration = (MapGeneration)target;
        
        if (DrawDefaultInspector()) // Check if any value was changed
        {
            if (mapGeneration.updateAutomatically)
            {
                mapGeneration.InitializeTerrainGradientColours();
                mapGeneration.GenerateMap(); // Generate the map
            }
        }

        if (GUILayout.Button("Generate")) // If editor button 'Generate' is clicked
        {
            mapGeneration.InitializeTerrainGradientColours();
            mapGeneration.GenerateMap(); // Generate the map
        }
    }
}