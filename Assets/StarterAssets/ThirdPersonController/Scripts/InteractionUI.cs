using System;
using UnityEngine;
using TMPro;
public class InteractionUI : MonoBehaviour
{
    private PlayerInteraction _playerInteraction;
    
    [SerializeField] private GameObject interactPanel;
    [SerializeField] private TextMeshProUGUI interactText;

    private void Awake()
    {
        _playerInteraction = GetComponent<PlayerInteraction>();
    }

    private void Update()
    {
        if (_playerInteraction.CanInteract && _playerInteraction._closestInteractable != null)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        interactPanel.SetActive(true);
        
        interactText.SetText("Press E to " + _playerInteraction._closestInteractable.GetName());
        interactText.gameObject.SetActive(true);
    }

    private void Hide()
    {
        interactPanel.SetActive(false);
        interactText.gameObject.SetActive(false);
        interactText.SetText("");
        
    }
    
}