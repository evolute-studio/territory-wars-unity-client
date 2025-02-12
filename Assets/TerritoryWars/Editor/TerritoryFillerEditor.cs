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
            // Малюємо стандартний інспектор
            DrawDefaultInspector();

            // Додаємо розділювач
            EditorGUILayout.Space(10);

            // Починаємо бокс для кнопок
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Заголовок боксу
            EditorGUILayout.LabelField("Fence Placement", EditorStyles.boldLabel);

            TerritoryFiller territoryFiller = (TerritoryFiller)target;

            // Кнопка для розміщення забору
            if (GUILayout.Button("Fill Territory"))
            {
                territoryFiller.PlaceTerritory();
            }

            // Закінчуємо бокс
            EditorGUILayout.EndVertical();
        }
    }
}