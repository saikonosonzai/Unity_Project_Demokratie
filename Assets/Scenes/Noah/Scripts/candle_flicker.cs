using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    public Light candleLight;

    public float baseIntensity = 2.0f;
    public float flickerAmount = 0.15f; // ±7.5%
    public float flickerSpeed = 15f;

    private float targetIntensity;

    void Start()
    {
        targetIntensity = baseIntensity;
    }

    void Update()
    {
        if (Mathf.Abs(candleLight.intensity - targetIntensity) < 0.02f)
        {
            targetIntensity = baseIntensity + Random.Range(-flickerAmount, flickerAmount);
        }

        candleLight.intensity = Mathf.Lerp(
            candleLight.intensity,
            targetIntensity,
            Time.deltaTime * flickerSpeed
        );
    }
}