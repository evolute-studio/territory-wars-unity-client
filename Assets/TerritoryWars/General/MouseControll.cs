using UnityEngine;

namespace TerritoryWars.General
{

    public class MouseControll : MonoBehaviour
    {
        [Header("Camera Movement")] [SerializeField]
        private float panSpeed = 20f;

        [SerializeField] private float zoomSpeed = 20f;
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 4f;

        private Camera mainCamera;
        private Vector3 lastMousePosition;
        private bool isDragging = false;
        private Vector3 minBounds = new Vector3(-4f, -3f, 0f);
        private Vector3 maxBounds = new Vector3(4f, 4f, 0f);

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
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                Vector3 move = new Vector3(-delta.x, -delta.y, 0) * panSpeed * Time.deltaTime;
                move *= mainCamera.orthographicSize / 5f;

                Vector3 newPosition = transform.position + move;
                newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);

                transform.position = newPosition;
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