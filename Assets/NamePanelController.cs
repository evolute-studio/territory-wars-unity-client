using System;
using TerritoryWars;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NamePanelController : MonoBehaviour
{
    public GameObject NamePanel;
    public GameObject ChangeNamePanel;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI EvoluteCountText;
    public Button ChangeNameButton;
    
    private bool _isInitialized = false;
    
    public void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }
        //ChangeNameButton.onClick.AddListener(CallChangeNamePanel);

        evolute_duel_Player profile = DojoGameManager.Instance.GetLocalPlayerData();
        if(profile == null)
        {
            CustomLogger.LogWarning("profile is null");
            
            DojoGameManager.Instance.SetPlayerName(CairoFieldsConverter.GetFieldElementFromString("Default" + UnityEngine.Random.Range(0, 1000)));
            SetName(DojoGameManager.Instance.LocalBurnerAccount.Address.Hex());
            SetEvoluteBalance(0);
            return;
        }
        else
        {
            string name = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
            SetName(name);
            SetEvoluteBalance(profile.balance);
        }
        
    }

    public void CallChangeNamePanel()
    {
        ChangeNamePanel.SetActive(true);
    }

    public void SetName(string name)
    {
        PlayerNameText.text = name;
    }
    
    public void SetEvoluteBalance(int value){
        EvoluteCountText.text = value.ToString();
    }
}
