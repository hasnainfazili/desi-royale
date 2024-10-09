using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterSelectionUI : MonoBehaviour
{
    public GameObject characterPanelPrefab;

    public GameObject activeCharacter;
    public void CreateCharacterUI(CharacterSO character)
    {
        Instantiate(characterPanelPrefab, transform);
        characterPanelPrefab.GetComponentInChildren<TextMeshProUGUI>().text = character.characterName;
        characterPanelPrefab.GetComponent<Image>().sprite = character.sprite;
    }

    public void SetActiveCharacter(CharacterSO character)
    {
        // IDK Brain Done
    }
}