using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class RCON
{
    protected Socket S;
    protected byte[] PacketsReceived;
    protected const int FIONREAD = 1074030207;
    protected bool m_connected;

    public RCON()
    {
        S = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public bool Connect(IPEndPoint Server, string password)
    {
        try
        {
            S = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            S.Connect(Server);
        }
        catch
        {
            m_connected = false;
            return false;
        }
		
        bool Connect;
		
        if (rconAuth(password))
        {
            m_connected = true;
            Connect = true;
        }
        else
        {
            m_connected = false;
            Connect = false;
        }

        return Connect;
    }

    protected bool rconAuth(string pw)
    {
        if (S.Connected)
        {
            if (SyncSend(new RCONPacket
            {
                RequestId = 1,
                String1 = pw,
                ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_AUTH
            }) && (PacketsReceived[4] == 1 & PacketsReceived.Length > 22) && PacketsReceived[22] == 2 && PacketsReceived[21] == 0)
            {
                return true;
            }
        }
        else if (!S.Connected & m_connected)
            m_connected = false;

        return false;
    }

    public bool Disconnect()
    {
        try
        {
            S.Dispose();
        }
        catch { }

        bool Disconnect;
        if (!S.Connected)
        {
            m_connected = false;
            Disconnect = true;
        }
        else
            Disconnect = false;

        return Disconnect;
    }

    public string RSendCommand(string command)
    {
        string RSendCommand;
        if (S.Connected && SyncSend(new RCONPacket
        {
            RequestId = 2,
            ServerDataSent = RCONPacket.SERVERDATA_sent.SERVERDATA_EXECCOMMAND,
            String1 = command
        }) && (PacketsReceived[4] == 2 & PacketsReceived.Length > (int)PacketsReceived[0]))
            RSendCommand = Encoding.ASCII.GetString(PacketsReceived, 12, checked(PacketsReceived.Length - 12));
        else
            RSendCommand = "";

        return RSendCommand;
    }

    protected bool SyncSend(RCONPacket p)
    {
        byte[] Packets = p.OutputAsBytes();

        checked
        {
            if (S.Connected)
            {
                if (S.Send(Packets, 0, Packets.Length, SocketFlags.None) == Packets.Length)
                {
                    Thread.Sleep(1000);
                    int iCount = 0;

                    while (S.Available < 4 & iCount < 5)
                    {
                        Thread.Sleep(1000);
                        iCount++;
                    }

                    PacketsReceived = new byte[S.Available - 1 + 1];
                    int iIndex = 0;

                    while (S.Available > 0)
                    {
                        int iRec = S.Receive(PacketsReceived, iIndex, S.Available, SocketFlags.None);
                        iIndex += iRec;
                    }

                    return iIndex > 0;
                }
            }
            else if (!S.Connected & m_connected)
                m_connected = false;

            return false;
        }
    }
}

public class RCONPacket
{
    internal RCONPacket()
    {
        RequestId = 0;
        String1 = "blah";
        String2 = string.Empty;
        ServerDataSent = SERVERDATA_sent.None;
        ServerDataReceived = SERVERDATA_rec.None;
    }

    internal byte[] OutputAsBytes()
    {
        UTF8Encoding utf8Encoding = new UTF8Encoding();
        byte[] bstring = utf8Encoding.GetBytes(this.String1);
        byte[] bstring2 = utf8Encoding.GetBytes(this.String2);
        byte[] serverdata = BitConverter.GetBytes((int)this.ServerDataSent);
        byte[] reqid = BitConverter.GetBytes(this.RequestId);

        checked
        {
            byte[] FinalPacket = new byte[12 + bstring.Length + 1 + bstring2.Length + 1];
            Array bytes = BitConverter.GetBytes(FinalPacket.Length - 4);
            int BPtr = 0;
            bytes.CopyTo(FinalPacket, BPtr);
            BPtr += 4;
            reqid.CopyTo(FinalPacket, BPtr);
            BPtr += 4;
            serverdata.CopyTo(FinalPacket, BPtr);
            BPtr += 4;
            bstring.CopyTo(FinalPacket, BPtr);
            BPtr += bstring.Length;
            FinalPacket[BPtr] = 0;
            BPtr++;
            bstring2.CopyTo(FinalPacket, BPtr);
            BPtr += bstring2.Length;
            FinalPacket[BPtr] = 0;
            BPtr++;

            return FinalPacket;
        }
    }

    internal int RequestId;
    internal string String1;
    internal string String2;
    internal SERVERDATA_sent ServerDataSent;
    internal SERVERDATA_rec ServerDataReceived;
	
    public enum SERVERDATA_sent
    {
        SERVERDATA_AUTH = 3,
        SERVERDATA_EXECCOMMAND = 2,
        None = 255
    }
	
    public enum SERVERDATA_rec
    {
        SERVERDATA_RESPONSE_VALUE,
        SERVERDATA_AUTH_RESPONSE = 2,
        None = 255
    }
}