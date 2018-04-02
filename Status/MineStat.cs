using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

public class ServerStatusChecker
{
    ushort pckSize = 512; // this will hopefully suffice since the MotD should be <=59 characters
    ushort fieldCnt = 6;  // number of values expected from server

    public string Address { get; set; }
    public ushort Port { get; set; }
    public string Motd { get; set; }
    public string Version { get; set; }
    public string CurrentPlayers { get; set; }
    public string MaximumPlayers { get; set; }
    public bool isOnline { get; set; }
    public long Delay { get; set; }

    public ServerStatusChecker(string adr, ushort port)
    {
        var rawServerData = new byte[pckSize];

        Address = adr;
        Port = port;

        try
        {
            var pt = new Stopwatch();
            var cl = new TcpClient();
            pt.Start();
            cl.Connect(adr, port);
            pt.Stop();

            var stream = cl.GetStream();
            var payload = new byte[] { 0xFE, 0x01 };

            stream.Write(payload, 0, payload.Length);
            stream.Read(rawServerData, 0, pckSize);

            cl.Close();
            Delay = pt.ElapsedMilliseconds;
        }
        catch (Exception)
        {
            isOnline = false;
            return;
        }

        if (rawServerData == null || rawServerData.Length == 0)
        {
            isOnline = false;
        }
        else
        {
            var splitDatas = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
            if (splitDatas != null && splitDatas.Length >= fieldCnt)
            {
                isOnline = true;
                Version = splitDatas[2];
                Motd = splitDatas[3];
                CurrentPlayers = splitDatas[4];
                MaximumPlayers = splitDatas[5];
            }
            else
            {
                isOnline = false;
            }
        }
    }

    #region Obsolete

    public string GetAddress()
    {
        return Address;
    }
    public void SetAddress(string address)
    {
        Address = address;
    }
    public ushort GetPort()
    {
        return Port;
    }
    public void SetPort(ushort port)
    {
        Port = port;
    }
    public string GetMotd()
    {
        return Motd;
    }
    public void SetMotd(string motd)
    {
        Motd = motd;
    }
    public string GetVersion()
    {
        return Version;
    }
    public void SetVersion(string version)
    {
        Version = version;
    }
    public string GetCurrentPlayers()
    {
        return CurrentPlayers;
    }
    public void SetCurrentPlayers(string currentPlayers)
    {
        CurrentPlayers = currentPlayers;
    }
    public string GetMaximumPlayers()
    {
        return MaximumPlayers;
    }
    public void SetMaximumPlayers(string maximumPlayers)
    {
        MaximumPlayers = maximumPlayers;
    }
    public bool IsServerUp()
    {
        return isOnline;
    }

    #endregion
}