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

        public void Update()
        {
            if (!LoadingScreenObject.activeSelf) return;
            float time = Time.time * 2;
            int dotsCount = (int)(time % 4); 
            loadingText.text = loadingTextPrefix + new string('.', dotsCount) + new string(' ', 3 - dotsCount);
        }
        
        public void SetActive(bool active, Action onCancel = null, bool isTextEnabled = true)
        {
            loadingText.gameObject.SetActive(isTextEnabled);
            
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