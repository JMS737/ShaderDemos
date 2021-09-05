using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab = default;

    [SerializeField]
    [Range(10, 100)]
    int resolution = 20;

    [SerializeField]
    FunctionLibrary.FunctionName function = default;

    public enum TransitionMode { Cycle, Random }
    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField]
    float functionDuration = 1f;


    [SerializeField]
    float transistionDuration = 1f;

    bool transitioning;
    FunctionLibrary.FunctionName transistionFunction;

    Transform[] points;

    float duration;


    void Awake()
    {
        points = new Transform[resolution * resolution];

        var step = 2f / resolution;
        var scale = Vector3.one * step;
        for (int i = 0; i < points.Length; i++)
        {
            var point = Instantiate(pointPrefab, transform, false);
            point.localScale = scale;
            points[i] = point;
        }
    }

    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transistionDuration)
            {
                duration -= transistionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transistionFunction = function;
            PickNextFunction();
        }

        if (transitioning)
            UpdateFunctionTransition();
        else
            UpdateFunction();
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }

    void UpdateFunction()
    {
        var time = Time.time;
        var f = FunctionLibrary.GetFunction(function);

        var step = 2f / resolution;
        float v = (0.5f) * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
                v = (z + 0.5f) * step - 1f;
            }
            
            float u = (x + 0.5f) * step - 1f;

            points[i].localPosition =  f(u, v, time);
        }
    }

    void UpdateFunctionTransition()
    {
        var from = FunctionLibrary.GetFunction(transistionFunction);
        var to = FunctionLibrary.GetFunction(function);
        var progress = duration / transistionDuration;
        
        var time = Time.time;

        var step = 2f / resolution;
        float v = (0.5f) * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
                v = (z + 0.5f) * step - 1f;
            }
            
            float u = (x + 0.5f) * step - 1f;

            points[i].localPosition =  FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }
}
