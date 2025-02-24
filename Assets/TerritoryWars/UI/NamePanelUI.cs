using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NamePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject NamePanel;
    [SerializeField] private TMP_InputField NameInputField;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    
    private string _name;

    private void Start()
    {
        Initialization();
    }

    private void Initialization()
    {
        _confirmButton.onClick.AddListener(GetNameFromInputField);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }
    
    public void SetNamePanelActive(bool active)
    {
        NamePanel.SetActive(active);
    }

    private void GetNameFromInputField()
    {
        _name = NameInputField.text;
    }

    private void OnCancelButtonClick()
    {
        SetNamePanelActive(false);
    }
}
