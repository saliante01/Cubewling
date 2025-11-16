using UnityEngine;
using UnityEngine.UI;

public class ParabolaSliderUI : MonoBehaviour
{
    public Slider slider;
    public LanzadorCajaController lanzador;

    private void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.minValue = lanzador.parabolaMin;
        slider.maxValue = lanzador.parabolaMax;
    }

    private void Update()
    {
        if (lanzador != null)
            slider.value = lanzador.parabolaAngle;
    }
}
