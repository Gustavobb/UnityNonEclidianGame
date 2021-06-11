using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowingCube : MonoBehaviour
{
    [SerializeField]
    Vector3 scaleMin, scaleMax;
    [SerializeField]
    float lerpDuration = .5f;
    bool inCoroutine = false, coroutineNeedsToStop = false;
    [SerializeField]
    GameObject Mesh;
    enum State {Grown, Shrinked};
    [SerializeField]
    State state;
    void Start()
    {
        Mesh.transform.localScale = scaleMin;
        if (state == State.Grown)
            Mesh.transform.localScale = scaleMax;
    }

    Vector3 HandleTrigger()
    {
        if (state == State.Grown)
        {
           state = State.Shrinked;
           return scaleMin;
        }
        else if (state == State.Shrinked)
        {
            state = State.Grown;
            return scaleMax;
        }

        return Mesh.transform.localScale;
    }

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Lerp(Mesh.transform.localScale, HandleTrigger()));
    }

    void OnTriggerExit(Collider other)
    {
        StartCoroutine(Lerp(Mesh.transform.localScale, HandleTrigger()));
    }

    IEnumerator Lerp(Vector3 startValue, Vector3 endValue)
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            Mesh.transform.localScale = Vector3.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        Mesh.transform.localScale = endValue;
    }
}
