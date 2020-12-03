using System.Collections;
using System.Collections.Generic;
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
            core.Connect("127.0.0.1", 5555);
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