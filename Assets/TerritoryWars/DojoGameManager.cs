using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using bottlenoselabs.C2CS.Runtime;
using Dojo;
using Dojo.Starknet;
using UnityEngine;

// Fix to use Records in Unity ref. https://stackoverflow.com/a/73100830
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace TerritoryWars
{
    public class DojoGameManager : MonoBehaviour
    {
        [SerializeField] WorldManager worldManager;

        [SerializeField] WorldManagerData dojoConfig;
        [SerializeField] GameManagerData gameManagerData;

        public BurnerManager burnerManager;

        public JsonRpcClient provider;
        public Account masterAccount;
        
        public Account LocalBurnerAccount { get; private set; }


        void Start()
        {
            provider = new JsonRpcClient(dojoConfig.rpcUrl);
            masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey),
                new FieldElement(gameManagerData.masterAddress));
            burnerManager = new BurnerManager(provider, masterAccount);

            CreateAccount();
        }
        
        private async void CreateAccount()
        {
            try
            {
                LocalBurnerAccount = await burnerManager.DeployBurner();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}