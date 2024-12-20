using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideInFog : MonoBehaviour
{
    [SerializeField] private LayerMask hideUnderLayer;
    private List<Renderer> renderers = new List<Renderer>();
    private void Update()
    {
        if (Physics.Raycast(new Ray(transform.position, Vector3.up), 20f, hideUnderLayer))
        {
            renderers.Clear();
            if (TryGetComponent<Renderer>(out Renderer render)) renderers.Add(render);
            var children = GetComponentsInChildren<Renderer>(true);
            renderers.AddRange(children);

            foreach (var child in renderers)
            {
                if (child == null) continue;

                child.enabled = false;
            }
        }
        else
        {
            foreach (var child in renderers)
            {
                if (child == null) continue;
                child.enabled = true;
            }
        }
    }
}
