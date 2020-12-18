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

namespace DevelopersHub.Unity.Networking
{
    internal static class Incoming
    {

        private enum IncomingType
        {
            connected = 0
        }

        internal static void PacketRouter()
        {
            Configuration.core.PacketId[(int)IncomingType.connected] = new Core.DataArgs(Connected);
        }

        private static void Connected(ref byte[] data)
        {
            Carrier carrier = new Carrier(data);
            int id = carrier.GetInt32();
            carrier.Dispose();
            Manager.instance.OnConnectionConfirmed(id);
        }

    }
}
