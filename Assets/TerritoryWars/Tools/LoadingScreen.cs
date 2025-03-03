using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.Tools
{
    public class LoadingScreen : MonoBehaviour
    {
        public bool IsLoading => LoadingScreenObject.activeSelf;
        
        public GameObject LoadingScreenObject;
        public Button CancelButton;
        public TextMeshProUGUI loadingText;
        public string loadingTextPrefix = "";
        public string connectingText = "Connecting to the game";

        private string currentText = "";

        public void Update()
        {
            if (!LoadingScreenObject.activeSelf) return;
            float time = Time.time * 2;
            int dotsCount = (int)(time % 4); 
            loadingText.text = currentText + new string('.', dotsCount) + new string(' ', 3 - dotsCount);
        }
        
        public void SetActive(bool active, Action onCancel = null, string text = "")
        {
            if(text != "")
            {
                loadingText.gameObject.SetActive(true);
                currentText = text;
            }
            else
            {
                loadingText.gameObject.SetActive(false);
            }
            
            
            LoadingScreenObject.SetActive(active);
            CancelButton.onClick.RemoveAllListeners();
            if (onCancel != null)
            {
                CancelButton.onClick.AddListener(() =>
                {
                    onCancel();
                    SetActive(false);
                });
            }
            CancelButton.gameObject.SetActive(onCancel != null);
        }
    }
}