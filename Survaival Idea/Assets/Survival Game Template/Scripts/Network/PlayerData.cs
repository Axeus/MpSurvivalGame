using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;


public class PlayerData : NetworkBehaviour {

    public string playerName;
    public string playerPassword;

    public bool isSingle;

    public void setName(string playerName)
    {
        this.playerName = playerName;
    }

    public void setPassword(string password)
    {
        this.playerPassword = GenerateHash(password, "377f976f0ad6a83bc444adfc7a41b0fa");
    }

    private static string GenerateHash(string value, string salt)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(salt + value);
        data = System.Security.Cryptography.MD5.Create().ComputeHash(data);
        return Convert.ToBase64String(data);
    }
}
