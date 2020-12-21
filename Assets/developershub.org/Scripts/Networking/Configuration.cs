//-----------------------------------------------------------------------
// Author  : Armin Ahmadi
// Email   : developershub.organization@gmail.com
// Website : www.developershub.org
// Copyright © 2020, Developers Hub
// All rights reserved
//-----------------------------------------------------------------------

using UnityEngine;

namespace DevelopersHub.Unity.Networking
{
    public class Configuration : MonoBehaviour
    {

        internal static Core core;
        internal static void InitializeNetwork()
        {
            if (!ReferenceEquals(core, null))
            {
                return;
            }
            core = new Core(100);
            Incoming.PacketRouter();
        }

        internal static void ConnectToServer()
        {
            core.ConnectionFailed += Core_ConnectionFailed;
            core.ConnectionLost += Core_ConnectionLost;
            core.ConnectionSuccess += Core_ConnectionSuccess;
            core.Connect("127.0.0.1", 5555);
        }

        private static void Core_ConnectionSuccess()
        {
            Manager.instance.OnConnected();
        }

        private static void Core_ConnectionLost()
        {
            Manager.instance.OnDisconnected();
        }

        private static void Core_ConnectionFailed()
        {
            Manager.instance.OnConnectFailedCall();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Unregister()
        {
            core.ConnectionFailed -= Core_ConnectionFailed;
            core.ConnectionLost -= Core_ConnectionLost;
            core.ConnectionSuccess -= Core_ConnectionSuccess;
        }

        internal static void DisconnectFromServer()
        {
            core.Dispose();
        }

        public static bool Connected
        {
            get
            {
                if (core == null)
                {
                    return false;
                }
                return core.IsConnected;
            }
        }

    }
}
