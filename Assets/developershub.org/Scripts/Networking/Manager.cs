//-----------------------------------------------------------------------
// Author  : Armin Ahmadi
// Email   : developershub.organization@gmail.com
// Website : www.developershub.org
// Copyright © 2020, Developers Hub
// All rights reserved
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using System.ComponentModel;

namespace DevelopersHub.Unity.Networking
{
    public class Manager : MonoBehaviour
    {

        #region Functions 
        public static Manager instance = null;
        private int connectionId = -1;
        private bool disconnectCalled = false;
        public int ConnectionID { get { return connectionId; } }
        protected System.ComponentModel.AsyncOperation asyncOperation;
        
        private void Awake()
        {
            Manager[] networkManagers = FindObjectsOfType<Manager>();
            if (networkManagers.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                asyncOperation = AsyncOperationManager.CreateOperation(null);
            }
        }

        private void Start()
        {
            DontDestroyOnLoad(this);
            Connect();
        }

        public void Connect()
        {
            if(connectionId >= 0)
            {
                return;
            }
            Configuration.InitializeNetwork();
            Configuration.ConnectToServer();
        }

        private void OnApplicationQuit()
        {
            if (connectionId > 0)
            {
                if (Configuration.Connected)
                {
                    Configuration.DisconnectFromServer();
                }
            }
        }
        
        public void OnConnectFailedCall()
        {
            asyncOperation.Post(OnConnectFailed, EventArgs.Empty);
        }

        private void OnConnectFailed(object state)
        {
            OnConnectFailed();
        }
        
        private void Update()
        {
            if (connectionId >= 0)
            {
                CheckDisconnect();
            }
        }

        private void CheckDisconnect()
        {
            if (!disconnectCalled)
            {
                if (!Configuration.Connected)
                {
                    disconnectCalled = true;
                    OnDisconnectedConfirmed();
                }
            }
        }
        #endregion

        #region Events
        public Events events = new Events();
        public void OnConnectionConfirmed(int id)
        {
            disconnectCalled = false;
            connectionId = id;
            events.onConnectedToServerConfirmed.Invoke();
        }

        public void OnDisconnected()
        {
            // connectionId = -1;
            // events.onDisconnectedFromServer.Invoke();
        }

        public void OnDisconnectedConfirmed()
        {
            connectionId = -1;
            events.onDisconnectedFromServer.Invoke();
        }

        public void OnConnected()
        {
            events.onConnectedToServer.Invoke();
        }

        public void OnConnectFailed()
        {
            connectionId = -1;
            events.onFailedToConnectToServer.Invoke();
        }

        [Serializable] public class Events
        {
            #region Manager Events
            public UnityEvent onConnectedToServer = new UnityEvent();
            public UnityEvent onConnectedToServerConfirmed = new UnityEvent();
            public UnityEvent onDisconnectedFromServer = new UnityEvent();
            public UnityEvent onFailedToConnectToServer = new UnityEvent();
            #endregion

            #region Client Incoming Events
            // You can create your own events here
            #endregion
        }
        #endregion
    }
}
