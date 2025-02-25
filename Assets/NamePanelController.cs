using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NamePanelController : MonoBehaviour
{
    public GameObject NamePanel;
    public GameObject ChangeNamePanel;
    private TextMeshProUGUI _playerNameText;
    private TextMeshProUGUI _evoluteCountText;
    private Button _changeNameButton;

    private void Start() => Initialize();
    
    private void Initialize()
    {
        _playerNameText = NamePanel.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>();
        _evoluteCountText = NamePanel.transform.Find("EvoluteCountText").GetComponent<TextMeshProUGUI>();
        _changeNameButton = NamePanel.transform.Find("ChangeNameButton").GetComponent<Button>();
        
        _changeNameButton.onClick.AddListener(CallChangeNamePanel);
    }

    public void CallChangeNamePanel()
    {
        ChangeNamePanel.SetActive(true);
    }

    public void ChangeName(string name)
    {
        _playerNameText.text = name;
    }
}
