using Unity.Netcode;
using UnityEngine;

public class NetworkManagerEvents : MonoBehaviour
{
    private void Start()
    {
        NetworkManager networkManager = GetComponent<NetworkManager>();
        networkManager.OnConnectionEvent += NetworkManager_OnConnectionEvent;
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
    {
        Debug.Log("Connection established");
#if !DEDICATED_SERVER
        Debug.Log("Hi");
#endif
    }
}
