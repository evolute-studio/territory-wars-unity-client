using System;
using UnityEngine;
using UnityEngine.UI;

public class AlertPopupUI : MonoBehaviour
{
    public GameObject AlerPopup;
    [SerializeField] private Button _confirmButton; 

    public void Start() => Initialize();

    public void Initialize()
    {
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
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
