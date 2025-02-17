using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using bottlenoselabs.C2CS.Runtime;
using Dojo;
using Dojo.Starknet;
using dojo_bindings;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = System.Object;
using Random = UnityEngine.Random;

// Fix to use Records in Unity ref. https://stackoverflow.com/a/73100830
using System.ComponentModel;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

public class GameManagerDojo : MonoBehaviour
{
    [SerializeField] WorldManager worldManager;

    [SerializeField] WorldManagerData dojoConfig;
    [SerializeField] GameManagerData gameManagerData;

    public BurnerManager burnerManager;
    private Dictionary<FieldElement, string> spawnedAccounts = new();
    public Actions actions;

    public JsonRpcClient provider;
    public Account masterAccount;


    void Start()
    {
        provider = new JsonRpcClient(dojoConfig.rpcUrl);
        masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey), new FieldElement(gameManagerData.masterAddress));
        burnerManager = new BurnerManager(provider, masterAccount);

        worldManager.synchronizationMaster.OnEntitySpawned.AddListener(InitEntity);
        var entities = worldManager.Entities<dojo_starter_Position>();
        Debug.Log("Entities: " + entities.Length);
        foreach (var entity in worldManager.Entities<dojo_starter_Position>())
        {
            InitEntity(entity);
        }
    }

    public async void SpawnEntity()
    {
        Debug.Log("SpawnEntity");
        var burner = await burnerManager.DeployBurner();
        spawnedAccounts[burner.Address] = null;
        var txHash = await actions.spawn(burner);
    }
    async void Update()
    {

        if (Input.GetKeyUp(KeyCode.E))
        {
            spawnedAccounts[masterAccount.Address] = null;
            var txHash = await actions.spawn(masterAccount);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            var burner = await burnerManager.DeployBurner();
            spawnedAccounts[burner.Address] = null;
            var txHash = await actions.spawn(burner);
        }

        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = Physics.Raycast(ray, out var hitInfo);
            if (hit && hitInfo.transform.parent != null)
            {
                var entity = hitInfo.transform.parent;
                dojo_starter_Position position;
                entity.TryGetComponent(out position);

                if (position && spawnedAccounts.ContainsValue(entity.name))
                {
                    var previousBurner = burnerManager.CurrentBurner;
                    if (previousBurner != null)
                    {
                        // worldManager.Entity(spawnedAccounts[previousBurner.Address])
                        //     .GetComponent<dojo_starter_Position>().textTag.color = Color.black;
                    }

                    var burner = spawnedAccounts.First(b => b.Value == entity.name);
                    var burnerAddress = burner.Key;
                    var burnerInstance = burnerManager.Burners.First(b => b.Address == burnerAddress);

                    //position.textTag.color = Color.blue;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            // coordinates are different between dojo world and unity
            Move(new Direction.Down());
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            Move(new Direction.Left());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Move(new Direction.Up());
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Move(new Direction.Right());
        }
    }
    
    public void MoveRight()
    {
        Debug.Log("MoveRight");
        Move(new Direction.Right());
    }
    
    public void MoveLeft()
    {
        Debug.Log("MoveLeft");
        Move(new Direction.Left());
    }

    public async void Move(Direction direction)
    {
        await actions.move(burnerManager.CurrentBurner ?? masterAccount, direction);
    }

    private async void InitEntity(GameObject entity)
    {
        await Task.Delay(1000);
        // check if entity has position component
        Debug.Log("InitEntity. Position: " + entity.GetComponent<dojo_starter_Position>());
        if (!entity.TryGetComponent(out dojo_starter_Position position)) return;

        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        // change color of capsule to a random color
        capsule.GetComponent<Renderer>().material.color = Random.ColorHSV();
        capsule.transform.parent = entity.transform;
        capsule.transform.localPosition = Vector3.zero;

        foreach (var account in spawnedAccounts)
        {
            if (account.Value == null)
            {
                spawnedAccounts[account.Key] = entity.name;
                break;
            }
        }
    }
}