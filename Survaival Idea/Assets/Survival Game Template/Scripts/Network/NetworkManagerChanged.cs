using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Networking.Match;

class NetworkManagerChanged : NetworkManager
{
    string playerName;

    bool isSingle;
    PlayerData data;

    private NetworkMatch networkMatch;

    public override void OnStopClient()
    {
        playerName = null;
    }

    void Update()
    {
        if (networkMatch == null)
        {
            var nm = GetComponent<NetworkMatch>();
            if (nm != null)
            {
                networkMatch = nm as NetworkMatch;
                UnityEngine.Networking.Types.AppID appid = (UnityEngine.Networking.Types.AppID)945402;
                networkMatch.SetProgramAppID(appid);
            }
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        conn.SetChannelOption(Channels.DefaultReliable, ChannelOption.MaxPendingBuffers, 500);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        isSingle = NetworkManager.FindObjectOfType<PlayerData>().isSingle;

        NetworkManager.FindObjectOfType<NetworkManagerHUDChanged>().enabled = false;

        NetworkManager manager =
        NetworkManager.FindObjectOfType<NetworkManager>();

        if (NetworkManager.FindObjectOfType<PlayerData>().playerName.Length < 3)
        {
            manager.StopClient();
            Debug.Log("Nickname must be longer than three characters.");
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {       
        if (playerName == null)
        {
            NetworkManager.FindObjectOfType<NetworkManagerHUDChanged>().enabled = false;
            data = NetworkManager.FindObjectOfType<PlayerData>();
            playerName = data.playerName;

            var player = (GameObject)GameObject.Instantiate(playerPrefab, NetworkManager.FindObjectOfType<NetworkStartPosition>().transform.position, Quaternion.identity);
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
        }
        else
        {
            if (!data.isSingle)
            {
                var player = (GameObject)GameObject.Instantiate(playerPrefab, NetworkManager.FindObjectOfType<NetworkStartPosition>().transform.position, Quaternion.identity);
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            }
            else
                conn.Disconnect();
        }       
    }
}