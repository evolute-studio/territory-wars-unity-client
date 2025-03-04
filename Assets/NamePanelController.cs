using System;
using System.Collections.Generic;
using TerritoryWars;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NamePanelController : MonoBehaviour
{
    public ChangeNamePanelUIController ChangeNamePanelUIController;
    
    public int EvoluteBalance;
    
    public GameObject NamePanel;
    public GameObject ChangeNamePanel;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI EvoluteCountText;
    public Button ChangeNameButton;
    
    private bool _isInitialized = false;

    // private void Awake()
    // {
    //     DojoGameManager.Instance.WorldManager.synchronizationMaster.OnSynchronized.AddListener(Initialize);
    // }

    // private void Initialize(List<GameObject> list)
    // {
    //     CustomLogger.LogWarning("Initialize NamePanelController after Sync");
    //     Invoke(nameof(Initialize), 0.1f);
    // }
    
    public void Initialize()
    {
        CustomLogger.LogWarning("Initialize NamePanelController");
        //ChangeNameButton.onClick.AddListener(CallChangeNamePanel);

        evolute_duel_Player profile = DojoGameManager.Instance.GetLocalPlayerData();
        if(profile == null)
        {
            CustomLogger.LogWarning("profile is null");
            
            string defaultName = DojoGameManager.Instance.LocalBurnerAccount.Address.Hex().Substring(0, 10);
            DojoGameManager.Instance.SetPlayerName(CairoFieldsConverter.GetFieldElementFromString(defaultName));
            SetName(defaultName);
            SetEvoluteBalance(0);
            return;
        }
        else
        {
            string name = CairoFieldsConverter.GetStringFromFieldElement(profile.username);
            SetName(name);
            SetEvoluteBalance(profile.balance);
            //DojoGameManager.Instance.WorldManager.synchronizationMaster.OnSynchronized.RemoveListener(Initialize);
        }
        
    }

    public void CallChangeNamePanel()
    {
        ChangeNamePanel.SetActive(true);
    }

    public bool IsDefaultName()
    {
        // default name starts with "0x"
        return PlayerNameText.text.StartsWith("0x");
    }
    
    public void SetName(string name)
    {
        PlayerNameText.text = name;
    }
    
    public void SetEvoluteBalance(int value)
    {
        EvoluteBalance = value;
        EvoluteCountText.text = " x " + value.ToString();
    }
}
