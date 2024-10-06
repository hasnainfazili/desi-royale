using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactRadius = 3f;
    [SerializeField] private int maxColliders = 3;
    
    [HideInInspector]
    public IInteractable _closestInteractable;
    
    public bool CanInteract {get; private set;}
    public void Interact()
    {
        if(_closestInteractable != null)
            _closestInteractable.Interact(transform);
    }
    
    private void Update()
    {
        _closestInteractable = GetClosestInteractable();
    }
    private IInteractable GetClosestInteractable()
    {
        var results = new Collider[maxColliders];
        Physics.OverlapSphereNonAlloc(transform.position, interactRadius, results);
        
        if (results.Length > 0)
        {
            CanInteract = true;
            foreach (var item in results)
            {
                if (item.TryGetComponent(out IInteractable interactable))
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
            }            
        }
        
        return _closestInteractable;
    }
}