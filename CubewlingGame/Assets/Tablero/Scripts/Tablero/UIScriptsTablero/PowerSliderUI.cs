using UnityEngine;
using UnityEngine.UI;

public class PowerSliderUI : MonoBehaviour
{
    public Slider slider;
    public LanzadorCajaController lanzador;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (lanzador == null) return;

        slider.maxValue = lanzador.maxPower;
        slider.value = lanzador.currentPower;
    }
}
