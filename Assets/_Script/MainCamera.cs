using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    static Portal[] portals;
    delegate void RenderPortals();
    static RenderPortals renderPortals;

    delegate void PostRenderPortals();
    static RenderPortals postRenderPortals;
    GravityBody playerGravityBody;

    void Awake() 
    {
        GetPortalCameras();
    }

    public static void GetPortalCameras()
    {
        portals = FindObjectsOfType<Portal>();
        renderPortals = null;
        postRenderPortals = null;

        for (int i = 0; i < portals.Length; i++)
        {
            renderPortals += portals[i].RenderCall;
            postRenderPortals += portals[i].PostRender;
        }
    }

    void OnPreCull() 
    {
        double lastInterval = Time.realtimeSinceStartup;
        if (renderPortals != null) 
        {
            renderPortals();
            postRenderPortals();
        }
        double timeNow = Time.realtimeSinceStartup;
        // Debug.Log(timeNow - lastInterval);
    }
}
