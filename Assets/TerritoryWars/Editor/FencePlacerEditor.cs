using TerritoryWars.Tile;
using UnityEditor;
using UnityEngine;

namespace TerritoryWars.Editor
{
    [CustomEditor(typeof(FencePlacer))]
    public class FencePlacerEditor : UnityEditor.Editor
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

            FencePlacer fencePlacer = (FencePlacer)target;

            // Кнопка для розміщення забору
            if (GUILayout.Button("Place Fence"))
            {
                fencePlacer.PlaceFence();
            }

            // Закінчуємо бокс
            EditorGUILayout.EndVertical();
        }
    }
}