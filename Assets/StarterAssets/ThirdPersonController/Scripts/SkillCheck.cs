using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCheck : MonoBehaviour
{
    public GameObject skillCheckPanel;
    
    public Slider backgroundSlider;
    public Slider movingSlider;
    public float skillCheckTimer = 10f;
    float maxSliderValue;
    float minSliderValue;
    public bool skillCheckActive = false;

    public void Start()
    {
        maxSliderValue = backgroundSlider.maxValue;
        minSliderValue = backgroundSlider.minValue;
    }
    
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
            DebugQTE();
        
    }

    public void FixedUpdate()
    {
        if (skillCheckActive)
        {
            StartCoroutine(SkillCheckCoroutine());
            if (Input.GetKeyDown(KeyCode.G))
            {
                var stoppedValue = movingSlider.value;
                var minCheckValue = backgroundSlider.value - _errorThreshold;
                var maxCheckValue = backgroundSlider.value + _errorThreshold;
                if (stoppedValue >= minCheckValue && stoppedValue <= maxCheckValue) // Add in a threshold
                {
                    skillCheckActive = false;
                    print("Skill check successful");
                }
                else
                {
                    print("Skill check failed");
                }
            }
        }
        
    }

    void DebugQTE()
    {
        backgroundSlider.value = Random.Range(minSliderValue, maxSliderValue);
        if(!skillCheckActive)
            skillCheckActive = true;
        else
        {
            skillCheckActive = false;
        }
    }
    
    //Call the skill check 
    //passing through or playing an animation within its context

    private float _sliderSpeed = .01f;
    [SerializeField] private  float _errorThreshold = 1f;
    IEnumerator SkillCheckCoroutine()
    {

        while (skillCheckActive)
        {
            skillCheckPanel.SetActive(true);
            Mathf.Clamp(movingSlider.value, minSliderValue, maxSliderValue);
            if(movingSlider.value >= maxSliderValue || movingSlider.value <= minSliderValue) 
                _sliderSpeed = -_sliderSpeed;
            movingSlider.value += _sliderSpeed;
            
            //Place a slider on a random value
            
            yield return null;
        }

        yield return new WaitForSeconds(skillCheckTimer);
        skillCheckActive = false;
    }
}