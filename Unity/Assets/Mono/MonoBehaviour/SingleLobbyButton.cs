using System;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SingleLobbyButton: MonoBehaviour
{
    public Text ID, Name, MumberCount;
    public Button button;
    public SteamId lobbyId;
    public string ServerId;

    public void Add(Action<string,SteamId> action)
    {
        button.onClick.AddListener(() =>
        {
            var id = this.ServerId;
            var lobbyId = this.lobbyId;
            action?.Invoke(id,lobbyId);
        });
    }
}