using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class InputCommand
{
    public string Name;
    public float Value;
}

public class AutomaticInputSystem : MonoBehaviour
{
    public bool UseInputAsDefault = true;

    public List<InputCommand> Axes = new List<InputCommand>()
    {
        new InputCommand { Name = "Vertical", Value = 0.0f },
        new InputCommand { Name = "Horizontal", Value = 0.0f }
    };
    Dictionary<string, float> AutoAxis = new Dictionary<string, float>();

    void Update()
    {
        foreach (var Inputs in Axes)
        {
            AutoAxis[Inputs.Name] = Inputs.Value;
        }

        var KeysToRemove = AutoAxis.Keys.Where(Key => Axes.FindIndex(x => x.Name == Key) < 0);

        foreach(var Key in KeysToRemove)
        {
            AutoAxis.Remove(Key);
        }

    }

    public float GetAxis(string name) => AutoAxis.ContainsKey(name) ? AutoAxis[name] : UseInputAsDefault ? Input.GetAxis(name) : 0.0f;
}
