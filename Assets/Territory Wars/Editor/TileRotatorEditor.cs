using UnityEditor;
using UnityEngine;

namespace TerritoryWars.Editor
{
    [CustomEditor(typeof(TileRotator))]
    public class TileRotatorEditor : UnityEditor.Editor
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
            EditorGUILayout.LabelField("Rotation Control", EditorStyles.boldLabel);

            TileRotator tileRotator = (TileRotator)target;

            // Кнопка для повороту за годинниковою стрілкою
            if (GUILayout.Button("Rotate Clockwise"))
            {
                tileRotator.RotateClockwise();
                EditorUtility.SetDirty(target);
            }

            // Закінчуємо бокс
            EditorGUILayout.EndVertical();
        }
    }
}