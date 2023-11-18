using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGeneration))]
    public class MapGenerationEditor : UnityEditor.Editor 
    {
        public override void OnInspectorGUI()
        {
            MapGeneration mapGeneration = (MapGeneration)target;

            if (DrawDefaultInspector()) // Check if any value was changed
            {
                if (mapGeneration.updateAutomatically) // Check if the map generation is set to update automatically
                {
                    mapGeneration.GenerateMap(); // Generate the map
                }
            }

            if (GUILayout.Button("Generate")) // If editor button 'Generate' is clicked
            {
                mapGeneration.GenerateMap(); // Generate the map
            }
        }
    }
}
