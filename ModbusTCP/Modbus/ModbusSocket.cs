using System;
using System.Threading;
using System.Net.Sockets;

namespace ModbusTCP
{
    public class ModbusSocket
    {
        private Socket socket;

        private int lastError;

        public bool Connected => socket != null && socket.Connected;

        public int ReceiveTimeout { get; set; }

        public int SendTimeout { get; set; }

        public int ConnectTimeout { get; set; }

        public ModbusSocket()
        {
        }

        ~ModbusSocket()
        {
            Close();
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
            }
        }

        private void CreateSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;            
        }

        private void TCPPing(string host, int port)
        {
            lastError = 0;
            Socket pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {

                IAsyncResult result = pingSocket.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(ConnectTimeout, true);

                if (!success)
                {
                    lastError = ModbusConstants.ExcTCPConnectionFailed;
                }
            }
            catch
            {
                lastError = ModbusConstants.ExcTCPConnectionFailed;
            }

            pingSocket.Close();
        }

        public int Connect(string host, int port)
        {
            lastError = 0;
            if (!Connected)
            {
                TCPPing(host, port);
                if (lastError == 0)
                {
                    try
                    {
                        CreateSocket();                     
                        IAsyncResult result = socket.BeginConnect(host, port, null, null);
                        bool isSuccess = result.AsyncWaitHandle.WaitOne(ConnectTimeout, true);
                        if (isSuccess && socket.Connected)
                        {
                            socket.EndConnect(result);
                        }
                        else
                        {
                            socket.Close();
                            lastError = ModbusConstants.ExcTCPConnectionFailed;
                        }
                    }
                    catch
                    {
                        lastError = ModbusConstants.ExcTCPConnectionFailed;
                    }                    
                }
                    
            }
            return lastError;
        }
        
        public int Send(byte[] buffer, int offset, int size)
        {
            int startTickCount = Environment.TickCount;
            int sent = 0;
            do
            {
                if (this.socket is null) return ModbusConstants.ExcTCPSocketCreation;
                if (Environment.TickCount > startTickCount + SendTimeout)
                    return ModbusConstants.ExcTCPSendTimeout;                
                try
                {
                    sent += this.socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {                        
                        Thread.Sleep(30);
                    }
                    else
                        return ModbusConstants.ExcTCPDataSend;
                }
            } while (sent < size);
            return ModbusConstants.ResultOK;
        }

        public int Receive(byte[] buffer, int offset, int size)
        {
            int startTickCount = Environment.TickCount;
            int received = 0;
            do
            {
                if (this.socket is null) return ModbusConstants.ExcTCPSocketCreation;
                if (Environment.TickCount > startTickCount + ReceiveTimeout)
                    return ModbusConstants.ExcTCPReceiveTimeout;
                try
                {
                    received += this.socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {                        
                        Thread.Sleep(30);
                    }
                    else
                        return ModbusConstants.ExcTCPDataReceive;
                }
            } while (received < size);
            return ModbusConstants.ResultOK;
        }

    }
}
