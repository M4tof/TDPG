using UnityEditor;

namespace TDPG.Templates.Grid.MapGen
{
    /// <summary>
    /// A Custom Inspector for the <see cref="MapGenerator"/> component.
    /// <br/>
    /// Provides dynamic UI sliders for "Water Level" and "Wall Level" that automatically adjust their 
    /// Min/Max ranges based on the selected <see cref="MapTypes"/>.
    /// </summary>
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        /// <summary>
        /// Overrides the default Inspector drawing logic to inject custom range clamping.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var mapType      = serializedObject.FindProperty("mapType");
            var waterLevel   = serializedObject.FindProperty("waterLevel");
            var wallLevel    = serializedObject.FindProperty("wallLevel");

            EditorGUILayout.PropertyField(mapType);

            // Dynamic ranges depending on mapType
            float minWater = -1f;
            float maxWater = -0.001f;

            float minWall = 0.001f;
            float maxWall = 1f;

            switch ((MapTypes)mapType.enumValueIndex)
            {
                case MapTypes.Smooth:
                    minWater = -1f;
                    maxWater = -0.09f;
                    minWall = 0.09f;
                    maxWall = 1f;
                    break;

                case MapTypes.Chaotic:
                    minWater = -1f;
                    maxWater = -0.15f;  
                    minWall = 0.1f;
                    maxWall = 1f;
                    break;

                case MapTypes.Mountainous:
                    minWater = -1f;
                    maxWater = -0.2f;
                    minWall = 0.3f;
                    maxWall = 1f;
                    break;
            }

            // Draw dynamic sliders
            waterLevel.floatValue =
                EditorGUILayout.Slider("Water Level", waterLevel.floatValue, minWater, maxWater);

            wallLevel.floatValue =
                EditorGUILayout.Slider("Wall Level", wallLevel.floatValue, minWall, maxWall);

            // Draw all the other fields
            DrawPropertiesExcluding(serializedObject, "mapType", "waterLevel", "wallLevel", "m_Script");

            serializedObject.ApplyModifiedProperties();
        }
    }
}