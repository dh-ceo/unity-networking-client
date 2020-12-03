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
                //ByteBuffer buffer = new ByteBuffer(4);
                //buffer.WriteInt32((int)OutgoingType.example);
                //Configuration.core.SendData(buffer.Data, buffer.Head);
                //buffer.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

    }
}