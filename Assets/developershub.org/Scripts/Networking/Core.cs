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
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace DevelopersHub.Unity.Networking
{
    public sealed class Core : IDisposable
    {

        public event Core.ConnectionArgs ConnectionSuccess;
        public event Core.ConnectionArgs ConnectionFailed;
        public event Core.ConnectionArgs ConnectionLost;
        public event Core.CrashReportArgs CrashReport;
        public event Core.PacketInfoArgs PacketReceived;
        public event Core.TrafficInfoArgs TrafficReceived;
        private Socket _socket;
        private byte[] _receiveBuffer;
        private byte[] _packetRing;
        private int _packetCount;
        private bool _connecting;
        public Core.DataArgs[] PacketId;

        public Core(int packetCount)
        {
            Threading.initUnityThread(false);
            if (this._socket != null)
            {
                return;
            }
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this._packetCount = packetCount;
            this.PacketId = new Core.DataArgs[packetCount];
        }

        public void Connect(string ip, int port)
        {
            if (this._socket == null || this._socket.Connected || this._connecting)
            {
                return;
            }
            if (ip.ToLower() == "localhost")
            {
                this._socket.BeginConnect((EndPoint)new IPEndPoint(IPAddress.Parse("127.0.0.1"), port), new AsyncCallback(this.DoConnect), (object)null);
            }
            else
            {
                this._socket.BeginConnect((EndPoint)new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(this.DoConnect), (object)null);
            }
        }

        private void DoConnect(IAsyncResult ar)
        {
            try
            {
                this._socket.EndConnect(ar);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Core.ConnectionArgs connectionFailed = this.ConnectionFailed;
                if (connectionFailed != null)
                {
                    connectionFailed();
                }
                this._connecting = false;
                return;
            }
            if (!this._socket.Connected)
            {
                Core.ConnectionArgs connectionFailed = this.ConnectionFailed;
                if (connectionFailed != null)
                {
                    connectionFailed();
                }
                this._connecting = false;
            }
            else
            {
                this._connecting = false;
                Core.ConnectionArgs connectionSuccess = this.ConnectionSuccess;
                if (connectionSuccess != null)
                {
                    connectionSuccess();
                }
                this._socket.ReceiveBufferSize = 8192;
                this._socket.SendBufferSize = 8192;
                this.BeginReceiveData();
            }
        }

        private void BeginReceiveData()
        {
            this._receiveBuffer = new byte[8192];
            this._socket.BeginReceive(this._receiveBuffer, 0, this._socket.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.DoReceive), (object)null);
        }

        private void DoReceive(IAsyncResult ar)
        {
            int length1;
            try
            {
                length1 = this._socket.EndReceive(ar);
            }
            catch
            {
                Core.CrashReportArgs crashReport = this.CrashReport;
                if (crashReport != null)
                {
                    crashReport("ConnectionForciblyClosedException");
                }
                this.Disconnect();
                return;
            }
            if (length1 < 1)
            {
                if (this._socket == null)
                {
                    return;
                }
                Core.CrashReportArgs crashReport = this.CrashReport;
                if (crashReport != null)
                {
                    crashReport("BufferUnderFlowException");
                }
                this.Disconnect();
            }
            else
            {
                Core.TrafficInfoArgs trafficReceived = this.TrafficReceived;
                if (trafficReceived != null)
                {
                    trafficReceived(length1, ref this._receiveBuffer);
                }
                if (this._packetRing == null)
                {
                    this._packetRing = new byte[length1];
                    Buffer.BlockCopy((Array)this._receiveBuffer, 0, (Array)this._packetRing, 0, length1);
                }
                else
                {
                    int length2 = this._packetRing.Length;
                    byte[] numArray = new byte[length1 + length2];
                    Buffer.BlockCopy((Array)this._packetRing, 0, (Array)numArray, 0, length2);
                    Buffer.BlockCopy((Array)this._receiveBuffer, 0, (Array)numArray, length2, length1);
                    this._packetRing = numArray;
                }
                this.PacketHandler();
                this._receiveBuffer = new byte[8192];
                this._socket.BeginReceive(this._receiveBuffer, 0, this._socket.ReceiveBufferSize, SocketFlags.None, new AsyncCallback(this.DoReceive), (object)null);
            }
        }

        private void PacketHandler()
        {
            int length = this._packetRing.Length;
            int num = 0;
            int count;
            while (true)
            {
                count = length - num;
                if (count >= 4)
                {
                    int int32_1 = BitConverter.ToInt32(this._packetRing, num);
                    if (int32_1 >= 4)
                    {
                        if (int32_1 <= count)
                        {
                            int startIndex = num + 4;
                            int int32_2 = BitConverter.ToInt32(this._packetRing, startIndex);
                            byte[] data;
                            if (int32_2 >= 0 && int32_2 < this._packetCount)
                            {
                                if (this.PacketId[int32_2] != null)
                                {
                                    if (int32_1 - 4 > 0)
                                    {
                                        data = new byte[int32_1 - 4];
                                        Buffer.BlockCopy((Array)this._packetRing, startIndex + 4, (Array)data, 0, int32_1 - 4);
                                        Threading.executeInUpdate((Action)(() =>
                                        {
                                            Core.PacketInfoArgs packetReceived = this.PacketReceived;
                                            if (packetReceived == null)
                                            {
                                                return;
                                            }
                                            packetReceived(int32_1 - 4, int32_2, ref data);
                                        }));
                                        Threading.executeInUpdate((Action)(() => this.PacketId[int32_2](ref data)));
                                        num = startIndex + int32_1;
                                    }
                                    else
                                    {
                                        data = new byte[0];
                                        Threading.executeInUpdate((Action)(() =>
                                        {
                                            Core.PacketInfoArgs packetReceived = this.PacketReceived;
                                            if (packetReceived == null)
                                            {
                                                return;
                                            }
                                            packetReceived(0, int32_2, ref data);
                                        }));
                                        Threading.executeInUpdate((Action)(() => this.PacketId[int32_2](ref data)));
                                        num = startIndex + int32_1;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                goto label_13;
                            }
                        }
                        else
                        {
                            goto label_19;
                        }
                    }
                    else
                    {
                        goto label_16;
                    }
                }
                else
                {
                    goto label_19;
                }
            }
            Core.CrashReportArgs crashReport1 = this.CrashReport;
            if (crashReport1 != null)
            {
                crashReport1("NullReferenceException");
            }
            this.Disconnect();
            return;
        label_13:
            Core.CrashReportArgs crashReport2 = this.CrashReport;
            if (crashReport2 != null)
            {
                crashReport2("IndexOutOfRangeException");
            }
            this.Disconnect();
            return;
        label_16:
            Core.CrashReportArgs crashReport3 = this.CrashReport;
            if (crashReport3 != null)
            {
                crashReport3("BrokenPacketException");
            }
            this.Disconnect();
            return;
        label_19:
            if (count == 0)
            {
                this._packetRing = (byte[])null;
            }
            else
            {
                byte[] numArray = new byte[count];
                Buffer.BlockCopy((Array)this._packetRing, num, (Array)numArray, 0, count);
                this._packetRing = numArray;
            }
        }

        public void SendData(byte[] data)
        {
            if (!this._socket.Connected)
            {
                return;
            }
            this._socket?.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(this.DoSend), (object)null);
        }

        public void SendData(byte[] data, int head)
        {
            if (!this._socket.Connected)
            {
                return;
            }
            byte[] buffer = new byte[head + 4];
            Buffer.BlockCopy((Array)BitConverter.GetBytes(head), 0, (Array)buffer, 0, 4);
            Buffer.BlockCopy((Array)data, 0, (Array)buffer, 4, head);
            this._socket?.BeginSend(buffer, 0, head + 4, SocketFlags.None, new AsyncCallback(this.DoSend), (object)null);
        }

        private void DoSend(IAsyncResult ar)
        {
            try
            {
                this._socket.EndSend(ar);
            }
            catch
            {
                Core.CrashReportArgs crashReport = this.CrashReport;
                if (crashReport != null)
                {
                    crashReport("ConnectionForciblyClosedException");
                }
                this.Disconnect();
            }
        }

        public bool IsConnected
        {
            get
            {
                return this._socket != null && this._socket.Connected;
            }
        }

        public void Disconnect()
        {
            if (this._socket == null || !this._socket.Connected)
            {
                return;
            }
            this._socket.BeginDisconnect(false, new AsyncCallback(this.DoDisconnect), (object)null);
        }

        private void DoDisconnect(IAsyncResult ar)
        {
            try
            {
                this._socket.EndDisconnect(ar);
            }
            catch
            {
            }
            Core.ConnectionArgs connectionLost = this.ConnectionLost;
            if (connectionLost == null)
            {
                return;
            }
            connectionLost();
        }

        public void Dispose()
        {
            this.Disconnect();
            this._socket.Close();
            this._socket.Dispose();
            this._socket = (Socket)null;
            this.PacketId = (Core.DataArgs[])null;
            this.ConnectionSuccess = (Core.ConnectionArgs)null;
            this.ConnectionFailed = (Core.ConnectionArgs)null;
            this.ConnectionLost = (Core.ConnectionArgs)null;
            this.CrashReport = (Core.CrashReportArgs)null;
            this.PacketReceived = (Core.PacketInfoArgs)null;
            this.TrafficReceived = (Core.TrafficInfoArgs)null;
            this.PacketId = (Core.DataArgs[])null;
        }

        public delegate void ConnectionArgs();
        public delegate void DataArgs(ref byte[] data);
        public delegate void CrashReportArgs(string reason);
        public delegate void PacketInfoArgs(int size, int header, ref byte[] data);
        public delegate void TrafficInfoArgs(int size, ref byte[] data);

    }
}
