using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowController : MonoBehaviour
{
    private Animator _animator;
    [Header("Throwable Settings")]
    [SerializeField] private Rigidbody currentThrowableEquipped;
    [SerializeField] private bool IsThrowAvailable = true;
    [SerializeField] private float ThrowDelay;
    [SerializeField] private AudioClip ThrowAudioClip;
    [SerializeField] private float ThrowStrength = 10f; 
    [SerializeField] private Transform _releaseTransform;

    //Reference to LineRenderer
    [Header("Line Renderer Display Settings")]
    [SerializeField] 
    private LineRenderer _lineRenderer;
    //Number of Points Created for the lineRenderer 
    //More points lead to smoother curve
    [SerializeField]
    [Range(10,100)]
    private int LinePoints;
    [SerializeField]
    [Range(.01f,.25f)]
    private float TimeBetweenPoints;


    // Reset the throwable back to release position
    private Transform InitialParent;
    private Vector3 InitialLocalPosition;
    private Quaternion InitialRotation;
    private void Awake()
    {
        _animator = GetComponent<Animator>();


        //Setting the reset position of the Throwable back to default for pooling
        InitialParent = currentThrowableEquipped.transform.parent;
        InitialRotation = currentThrowableEquipped.transform.localRotation;
        InitialLocalPosition = currentThrowableEquipped.transform.localPosition;
        currentThrowableEquipped.freezeRotation = true;
    }
    private void Update()
    {

        //If Right MB is pressed Allow the player to aim
        if (Application.isFocused && Mouse.current.rightButton.isPressed)
        {
            _animator.transform.rotation = Quaternion.Euler(
                _animator.transform.eulerAngles.x,
                Camera.main.transform.rotation.eulerAngles.y,
                _animator.transform.eulerAngles.z
            );
            // Draw the Trajectory of the object.
            DrawProjection();
            
            if (Mouse.current.leftButton.wasReleasedThisFrame && IsThrowAvailable)
            {
                //Set Throws to false 
                IsThrowAvailable = false;
                //Triggers the Throw Animation to Player
                _animator.SetTrigger("Throw Object");
            }
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
    private void DrawProjection()
    {
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = _releaseTransform.position;
        Vector3 startVelocity = ThrowStrength * Camera.main.transform.forward / currentThrowableEquipped.mass;
        int i = 0;
        _lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            _lineRenderer.SetPosition(i, point);

            Vector3 lastPosition = _lineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit,
                (point - lastPosition).magnitude))
            {
                _lineRenderer.SetPosition(i, hit.point);
                _lineRenderer.positionCount = i + 1;
                return;
            }
        }
    }

    //Function called by the Animation Event to throw the object
   
    private void ReleaseThrowable()
    {
        currentThrowableEquipped.velocity = Vector3.zero;
        currentThrowableEquipped.angularVelocity = Vector3.zero;
        currentThrowableEquipped.isKinematic = false;
        currentThrowableEquipped.freezeRotation = false;
        currentThrowableEquipped.transform.SetParent(null, true);
        currentThrowableEquipped.AddForce(Camera.main.transform.forward * ThrowStrength, ForceMode.Impulse);

        SoundFXManager.instance.PlaySingleSoundFXClip(ThrowAudioClip, transform, .4f);
        StartCoroutine(ResetThrowable());
    }
    //Coroutine resets the transform, rotation, and bools of the throwable
    private IEnumerator ResetThrowable()
    {
        yield return new WaitForSeconds(ThrowDelay);
        currentThrowableEquipped.freezeRotation = true;
        currentThrowableEquipped.isKinematic = true;
        currentThrowableEquipped.transform.SetParent(InitialParent, false);
        currentThrowableEquipped.rotation = InitialRotation;
        currentThrowableEquipped.transform.localPosition = InitialLocalPosition;
        IsThrowAvailable = true;
    }
}
