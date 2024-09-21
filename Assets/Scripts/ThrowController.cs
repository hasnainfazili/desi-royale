using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("_lineRenderer")]
    [Header("Line Renderer Display Settings")]
    [SerializeField] 
    private LineRenderer lineRenderer;
  
    [FormerlySerializedAs("LinePoints")]
    [SerializeField]
    [Range(10,100)]
    private int linePoints;
    [FormerlySerializedAs("TimeBetweenPoints")]
    [SerializeField]
    [Range(.01f,.25f)]
    private float timeBetweenPoints;


    // Reset the throwable back to release position
    private Transform _initialParent;
    private Vector3 _initialLocalPosition;
    private Quaternion _initialRotation;
    private void Awake()
    {
        _animator = GetComponent<Animator>();


        _initialParent = currentThrowableEquipped.transform.parent;
        _initialRotation = currentThrowableEquipped.transform.localRotation;
        _initialLocalPosition = currentThrowableEquipped.transform.localPosition;
        currentThrowableEquipped.freezeRotation = true;
    }
    private void Update()
    {

        if (Application.isFocused && Mouse.current.rightButton.isPressed)
        {
            currentThrowableEquipped.gameObject.SetActive(true);
            _animator.transform.rotation = Quaternion.Euler(
                _animator.transform.eulerAngles.x,
                Camera.main.transform.rotation.eulerAngles.y,
                _animator.transform.eulerAngles.z
            );
            DrawProjection();
            
            if (Mouse.current.leftButton.wasReleasedThisFrame && IsThrowAvailable)
            {
                IsThrowAvailable = false;
                _animator.SetTrigger("Throw Object");
            }
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
    private void DrawProjection()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        var startPosition = _releaseTransform.position;
        var startVelocity = ThrowStrength * Camera.main.transform.forward / currentThrowableEquipped.mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            var point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            lineRenderer.SetPosition(i, point);

            var lastPosition = lineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit,
                (point - lastPosition).magnitude))
            {
                lineRenderer.SetPosition(i, hit.point);
                lineRenderer.positionCount = i + 1;
                return;
            }
        }
    }

   
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
    private IEnumerator ResetThrowable()
    {
        yield return new WaitForSeconds(ThrowDelay);
        currentThrowableEquipped.freezeRotation = true;
        currentThrowableEquipped.isKinematic = true;
        currentThrowableEquipped.transform.SetParent(_initialParent, false);
        currentThrowableEquipped.rotation = _initialRotation;
        currentThrowableEquipped.transform.localPosition = _initialLocalPosition;
        IsThrowAvailable = true;
    }
}
