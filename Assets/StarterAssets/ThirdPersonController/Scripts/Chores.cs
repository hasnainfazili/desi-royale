using UnityEngine;

public class Chores : MonoBehaviour, IInteractable
{
    public void Interact(Transform interactor)
    {
        print("Interact " + name);
        //Add in skill check and animation for Interactions 
        //Lock player in the skill check
        //Show popup for the skill check
        //Play animation for the interactor
        
        //test with the run animation
    }

    public Transform GetTransform() => transform;
    public string GetName() => name;
}