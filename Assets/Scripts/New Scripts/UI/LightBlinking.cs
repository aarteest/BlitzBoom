using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightBlinking : MonoBehaviour
{
    bool loopComplete;

    [SerializeField]
    GameObject spotLight;

    [SerializeField]
    float minTime;

    [SerializeField]
    float maxTime;
    void Start()
    {
        loopComplete = true;
    }
    void Update()
    {
        if (loopComplete)
        {
            loopComplete = false;
            StartCoroutine(SwitchLights());
        }
    }

    private IEnumerator SwitchLights()
    {
        spotLight.SetActive(true);
        yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        spotLight.SetActive(false);
        yield return new WaitForSeconds(Random.Range(minTime, maxTime));
        loopComplete = true;
    }
}
