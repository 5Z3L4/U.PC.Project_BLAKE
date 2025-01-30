using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Respawnable
{
    public GameObject original;
    public Transform transform;

    public Respawnable(GameObject original, Transform transform)
    {
        this.original = original;
        this.transform = transform;
    }
}
