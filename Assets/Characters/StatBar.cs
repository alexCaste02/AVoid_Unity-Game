using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{

    [SerializeField] private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void UpdateStat(float currentStat, float maxStat)
    {
       
        slider.value = currentStat / maxStat;
        //Debug.Log("bar updated!");
    }

    public Slider getSlider()
    {
        return slider;
    }



}
