using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterSelection : MonoBehaviour
{
    private List<CharacterSO> totalCharacters;
    private List<CharacterSO> unlockedCharacters;
    
    
    private Character selectedCharacter;

    private void Start()
    {
        
    }
    
    //Populate and Create UI for each of the characters;

    private void CreateCharacterSelection()
    {
        foreach (var character in totalCharacters)
        {
            //Create a UI
            GetComponent<CharacterSelectionUI>().CreateCharacterUI(character);
        }
    }
}