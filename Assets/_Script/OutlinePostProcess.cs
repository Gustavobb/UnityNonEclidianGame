using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlinePostProcess : MonoBehaviour
{
    public Material postprocessMaterial;

    Camera cam;

    private void Start()
    {
        //get the camera and tell it to render a depthnormals texture
        cam = GetComponent<Camera>();
        if (postprocessMaterial != null) cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //draws the pixels from the source texture to the destination texture
        Graphics.Blit(source, destination, postprocessMaterial);
    }
}
