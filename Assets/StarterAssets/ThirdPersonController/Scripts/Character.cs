using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSelection", menuName = "Character Selection")]
public class CharacterSO : ScriptableObject
{
    public CharacterType characterType;
    
    public Sprite sprite;
    public string characterName;
}