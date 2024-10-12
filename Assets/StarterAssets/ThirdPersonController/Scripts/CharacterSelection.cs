using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterSelection : MonoBehaviour
{
    private List<CharacterSO> _totalCharacters;
    private List<CharacterSO> _unlockedCharacters;
    
    private Character _selectedCharacter;
    
    private void CreateCharacterSelection()
    {
        foreach (var character in _totalCharacters)
        {
            GetComponent<CharacterSelectionUI>().CreateCharacterUI(character);
        }
    }
}