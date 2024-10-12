using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    public GameObject characterPanelPrefab;
    public GameObject activeCharacterPanel;
    public void CreateCharacterUI(CharacterSO character)
    {
        Instantiate(characterPanelPrefab, transform);
        characterPanelPrefab.GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;
        characterPanelPrefab.GetComponent<Image>().sprite = character.sprite;
    }

    public void OnHoverCharacter(CharacterSO character)
    {
        activeCharacterPanel.GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;
        activeCharacterPanel.GetComponent<Image>().sprite = character.sprite;

    }
    public void SelectCharacter(CharacterSO character)
    {
        activeCharacterPanel.GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;
        activeCharacterPanel.GetComponent<Image>().sprite = character.sprite;
    }
    
    
}