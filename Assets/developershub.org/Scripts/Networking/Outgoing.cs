//-----------------------------------------------------------------------
// Author  : Armin Ahmadi
// Email   : developershub.organization@gmail.com
// Website : www.developershub.org
// Copyright © 2020, Developers Hub
// All rights reserved
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevelopersHub.Unity.Networking
{
    internal static class Outgoing
    {

        private enum OutgoingType
        {
            example = 0
        }

        public static void CheckLogin(int exampleInt, string exampleString)
        {
            try
            {
                //Carrier carrier = new Carrier(4);
                //carrier.SetInt32((int)OutgoingType.example);
                //Configuration.core.SendData(carrier.Data, carrier.Head);
                //carrier.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

    }
}
