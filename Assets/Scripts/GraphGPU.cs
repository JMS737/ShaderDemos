using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGPU : MonoBehaviour
{
    [SerializeField]
    [Range(10, 1000)]
    int resolution = 20;

    [SerializeField]
    ComputeShader computerShader = default;

    [SerializeField]
    Material material = default;

    [SerializeField]
    Mesh mesh = default;

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

    float duration;

    ComputeBuffer positionsBuffer;

    static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");

    void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(resolution * resolution, sizeof(float) * 3);
    }

    void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
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

        UpdateFunctionOnGPU();
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle ?
            FunctionLibrary.GetNextFunctionName(function) :
            FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computerShader.SetInt(resolutionId, resolution);
        computerShader.SetFloat(stepId, step);
        computerShader.SetFloat(timeId, Time.time);

        computerShader.SetBuffer(0, positionsId, positionsBuffer);

        int groups = Mathf.CeilToInt(resolution / 8f);
        computerShader.Dispatch(0, groups, groups, 1);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
    }
}
