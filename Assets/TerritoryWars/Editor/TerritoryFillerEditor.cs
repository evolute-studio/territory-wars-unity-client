using TerritoryWars.Tile;
using UnityEditor;
using UnityEngine;

namespace TerritoryWars.Editor
{
    [CustomEditor(typeof(TerritoryFiller))]
    public class TerritoryFillerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            
            EditorGUILayout.Space(10);

            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            
            EditorGUILayout.LabelField("Fence Placement", EditorStyles.boldLabel);

            TerritoryFiller territoryFiller = (TerritoryFiller)target;
            
            if (GUILayout.Button("Fill Territory"))
            {
                territoryFiller.PlaceTerritory();
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}