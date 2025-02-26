using System;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars;
using TerritoryWars.ModelsDataConverters;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SnapshotTabController : MonoBehaviour
{
    public GameObject PanelGameObject;
    public GameObject SnapshotListItemPrefab;
    public Transform ListItemParent;
    public GameObject BackgroundPlaceholderGO;
        
    private List<SnapshotListItem> _listItems = new List<SnapshotListItem>();
    
    public void Start() => Initialize();
        
        public void Initialize()
        {
            
        }
        
        public SnapshotListItem CreateListItem()
        {
            GameObject listItem = Instantiate(SnapshotListItemPrefab, ListItemParent);
            SnapshotListItem matchListItem = new SnapshotListItem(listItem);
            _listItems.Add(matchListItem);
            return matchListItem;
        }
        
        private void ClearAllListItems()
        {
            foreach (var matchListItem in _listItems)
            {
                Destroy(matchListItem.ListItem);
            }
            _listItems.Clear();
        }
        
        public void SetBackgroundPlaceholder(bool isActive)
        {
            BackgroundPlaceholderGO.SetActive(isActive);
        }

        private void CreatedNewEntity(GameObject newEntity)
        {
            if (!newEntity.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
            FetchData();
        }
        
        private void ModelUpdated(ModelInstance modelInstance)
        {
            if (!modelInstance.transform.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
            FetchData();
        }
        
        private void FetchData()
        {
            ClearAllListItems();
            GameObject[] snapshots = DojoGameManager.Instance.GetSnapshots();
            //BackgroundPlaceholderGO.SetActive(games.Length == 0);

            foreach (var snapshot in snapshots)
            {
                if (!snapshot.TryGetComponent(out evolute_duel_Snapshot snapshotModel)) return;
                SnapshotListItem snapshotListItem = CreateListItem();
                evolute_duel_Player player = DojoGameManager.Instance.GetPlayerData(snapshotModel.player.Hex());
                string playerName = CairoFieldsConverter.GetStringFromFieldElement(player.username);
                int evoluteBalance = player.balance;
                int moveNumber = snapshotModel.move_number;
                
                snapshotListItem.UpdateItem(playerName, evoluteBalance, moveNumber, () =>
                {
                    DojoGameManager.Instance.CreateGameFromSnapshot(snapshotModel.snapshot_id);
                });
                
            }

            SetBackgroundPlaceholder(snapshots.Length == 0);
        }
        
        public void SetActivePanel(bool isActive)
        {
            PanelGameObject.SetActive(isActive);
            if (isActive)
            {
                FetchData();
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(CreatedNewEntity);
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(ModelUpdated);
            }
            else
            {
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEntitySpawned.RemoveListener(CreatedNewEntity);
                DojoGameManager.Instance.WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(ModelUpdated);
                ClearAllListItems();
            }
        }
    
    
    
}

public class SnapshotListItem
{
    public GameObject ListItem;
    public string CreatorPlayerName;
    public int CreatorPlayerEvoluteCount;
    
    private TextMeshProUGUI _creatorPlayerNameText;
    private TextMeshProUGUI _creatorPlayerEvoluteCountText;
    private TextMeshProUGUI _tileLeftCountText;
    private Button _seeMapButton;
    private Button _restoreButton;

    public SnapshotListItem(GameObject listItem)
    {
        ListItem = listItem;
        _creatorPlayerNameText = listItem.transform.Find("PlayerName/PlayerNameText").GetComponent<TextMeshProUGUI>();
        _creatorPlayerEvoluteCountText = listItem.transform.Find("EvoluteCount/Count").GetComponent<TextMeshProUGUI>();
        _tileLeftCountText = listItem.transform.Find("TileLeft/TilesLeftText").GetComponent<TextMeshProUGUI>();
        _seeMapButton = listItem.transform.Find("Buttons/MapButton").GetComponent<Button>();
        _restoreButton = listItem.transform.Find("Buttons/RestoreButton").GetComponent<Button>();
    }

    public void UpdateItem(string creatorPlayerName, int creatorPlayerEvoluteCount, int moveNumber, UnityAction onRestore = null)
    {
        CreatorPlayerName = creatorPlayerName;
        CreatorPlayerEvoluteCount = creatorPlayerEvoluteCount;
        
        _creatorPlayerNameText.text = creatorPlayerName;
        _creatorPlayerEvoluteCountText.text = creatorPlayerEvoluteCount.ToString();
        _tileLeftCountText.text = "Move number: " + moveNumber;
        
        _restoreButton.onClick.RemoveAllListeners();
        if (onRestore != null)
        {
            _restoreButton.onClick.AddListener(onRestore);
        }
        
    }
    
    public void SetActive(bool isActive)
    {
        ListItem.SetActive(isActive);
    }
}
