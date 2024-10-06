using System.Collections.Generic;
using UnityEngine;
public interface IInteractable
{
    public void Interact(Transform interactorTransform);
    public Transform GetTransform();
    public string GetName();
}