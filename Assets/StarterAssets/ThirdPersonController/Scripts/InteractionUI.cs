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
        if (_playerInteraction.GetClosestInteractable() != null)
        {
            Show(_playerInteraction.GetClosestInteractable());
        }
        else
        {
            Hide();
        }
    }

    private void Show(IInteractable interactable)
    {
        interactPanel.SetActive(true);
        
        interactText.SetText("Press E to " + _playerInteraction.GetClosestInteractable().GetName());
        interactText.gameObject.SetActive(true);
    }

    private void Hide()
    {
        interactPanel.SetActive(false);
        interactText.gameObject.SetActive(false);
        interactText.SetText("");
        
    }
    
}