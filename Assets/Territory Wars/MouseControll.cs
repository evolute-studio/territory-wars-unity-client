using UnityEngine;

namespace TerritoryWars
{

    public class MouseControll : MonoBehaviour
    {
        [Header("Camera Movement")] [SerializeField]
        private float panSpeed = 20f;

        [SerializeField] private float zoomSpeed = 20f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 12f;

        private Camera mainCamera;
        private Vector3 lastMousePosition;
        private bool isDragging = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            HandlePanning();
            HandleZoom();
        }

        private void HandlePanning()
        {
            // Початок перетягування
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            // Кінець перетягування
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            // Переміщення камери при перетягуванні
            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;

                // Оскільки камера ортографічна, масштабуємо швидкість відносно зуму
                move *= mainCamera.orthographicSize / 5f;

                transform.Translate(move);
                lastMousePosition = Input.mousePosition;
            }
        }

        private void HandleZoom()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                float newSize = mainCamera.orthographicSize - scrollDelta * zoomSpeed * Time.deltaTime;
                mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }
    }
}