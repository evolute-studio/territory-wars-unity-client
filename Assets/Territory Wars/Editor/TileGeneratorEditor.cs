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
            // Малюємо стандартний інспектор
            DrawDefaultInspector();

            // Додаємо розділювач
            EditorGUILayout.Space(10);

            // Починаємо бокс для генерації тайлу
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Заголовок боксу
            EditorGUILayout.LabelField("Tile Generation", EditorStyles.boldLabel);

            // Отримуємо посилання на TileGenerator
            TileGenerator tileGenerator = (TileGenerator)target;

            // Поле для конфігурації тайлу
            tileGenerator.TileConfig = EditorGUILayout.TextField("Tile Config", tileGenerator.TileConfig);

            // Кнопка для генерації
            if (GUILayout.Button("Generate"))
            {
                tileGenerator.Generate();
                EditorUtility.SetDirty(target);
            }

            // Закінчуємо бокс
            EditorGUILayout.EndVertical();
        }
    }
}