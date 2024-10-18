using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private float interactRadius = 3f;
    [SerializeField] private int maxColliders = 3;

    [HideInInspector] public IInteractable _closestInteractable;

    public void Interact()
    {
        if (_closestInteractable != null)
            _closestInteractable.Interact(transform);
    }


    public IInteractable GetClosestInteractable()
    {
        List<IInteractable> interactablesinRange = new List<IInteractable>();

        var results = new Collider[maxColliders];
        Physics.OverlapSphereNonAlloc(transform.position, interactRadius, results);
        foreach (var item in results)
        {
            if (item.TryGetComponent(out IInteractable interactable))
            {
                interactablesinRange.Add(interactable);
            }
        }

        foreach (var interactable in interactablesinRange)
        {
            if (_closestInteractable == null)
            {
                _closestInteractable = interactable;
            }
            else
            {
                if (Vector3.Distance(transform.position, interactable.GetTransform().position)
                    < Vector3.Distance(transform.position, _closestInteractable.GetTransform().position))
                {
                    _closestInteractable = interactable;
                }
            }

        }
        return _closestInteractable;
    }
}