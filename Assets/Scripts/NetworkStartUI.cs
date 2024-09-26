using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class NetworkStartUI : MonoBehaviour
{
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;
    // Start is called before the first frame update
    void Start()
    {
        startHostButton.onClick.AddListener(StartHost);
        startClientButton.onClick.AddListener(StartClient);

    }

    void StartHost()
    {
        Debug.Log("Starting Host");
        
        NetworkManager.Singleton.StartHost();
        Hide();
    }

    void StartClient()
    {
        Debug.Log("Starting Client");
        
        NetworkManager.Singleton.StartClient();
        Hide();
    }


    private void Hide() => gameObject.SetActive(false);
}