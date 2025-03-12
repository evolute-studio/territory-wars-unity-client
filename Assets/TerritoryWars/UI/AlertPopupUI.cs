using System;
using UnityEngine;
using UnityEngine.UI;

public class AlertPopupUI : MonoBehaviour
{
    public GameObject AlerPopup;
    [SerializeField] private Button _confirmButton;
    public static bool IsGameCanceled;

    public void Start() => Initialize();

    public void Initialize()
    {
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        if (IsGameCanceled)
        {
            SetAlertPopupUIActive(true);
        }
    }

    public void SetAlertPopupUIActive(bool isActive)
    {
        AlerPopup.SetActive(isActive);
    }

    public void OnConfirmButtonClicked()
    {
        SetAlertPopupUIActive(false);
    }
    
    
}
