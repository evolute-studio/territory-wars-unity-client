using TerritoryWars.Tile;
using UnityEditor;
using UnityEngine;

namespace TerritoryWars.Editor
{
    [CustomEditor(typeof(TileRotator))]
    public class TileRotatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Rotation Control", EditorStyles.boldLabel);

            TileRotator tileRotator = (TileRotator)target;
            
            if (GUILayout.Button("Rotate Clockwise"))
            {
                tileRotator.RotateClockwise();
                EditorUtility.SetDirty(target);
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}