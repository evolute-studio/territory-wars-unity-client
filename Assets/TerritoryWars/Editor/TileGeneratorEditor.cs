using TerritoryWars.Tile;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TerritoryWars.Editor
{
    [CustomEditor(typeof(TileGenerator))]
    public class TileGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);

            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            
            EditorGUILayout.LabelField("Tile Generation", EditorStyles.boldLabel);

            
            TileGenerator tileGenerator = (TileGenerator)target;
            
            tileGenerator.TileConfig = EditorGUILayout.TextField("Tile Config", tileGenerator.TileConfig);
            
            if (GUILayout.Button("Generate"))
            {
                tileGenerator.Generate();
                EditorUtility.SetDirty(target);
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}