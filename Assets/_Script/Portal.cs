using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Portal : MonoBehaviour
{
    static readonly Vector3[] cubeCornerOffsets = {
        new Vector3 (1, 1, 1),
        new Vector3 (-1, 1, 1),
        new Vector3 (-1, -1, 1),
        new Vector3 (-1, -1, -1),
        new Vector3 (-1, 1, -1),
        new Vector3 (1, -1, -1),
        new Vector3 (1, 1, -1),
        new Vector3 (1, -1, 1),
    };

    public Portal portalToTeleportTo, portalToRender;
    [HideInInspector]
    public Camera playerCamera, selfCamera;
    public bool portalScaler = false; 
    [SerializeField]
    bool recursive = false;
    public bool doNotRender = false;
    [SerializeField]
    int recursionLimit = 5;
    
    [Header ("One Sided Portal Settings")]
    public bool oneSidedPortal = false;
    public enum RenderSide {Back, Front};
    public RenderSide renderSide;

    [Header ("Advanced Settings")]
    [SerializeField]
    [Tooltip("Frustum Culling. Note that this does not take ocluded objects into consideration. This is used to stop any portal that is not visible from rendering. 10x time gain. Not recommended for portals that can see other portals.")]
    bool cullPortalsNotInFrustum = true;
    [SerializeField]
    [Tooltip("Only use if frustum culling is active. Portals that can be seen also check if they can see portals. Fix the fact that frustum culling doesn't allow portals to see other portals not in view frustum. If portals can't be seen by others in your scene, do not use this.")]
    bool portalsCanSeeOtherPortals = false;
    [SerializeField]
    [Tooltip("Some optimization for recursive portals. Use only if self recusrive portals exist in scene.")]
    bool optimizeRecursivePortals = false;
    [SerializeField]
    [Tooltip("Sometimes it can clip the screen. Great performance. Use where needed.")]
    bool optimizeOneSidedPortals = false;

    [HideInInspector]
    public bool canBeSeen = true, needsToRenderPlane = true, canBeSeenByOtherPortal = false;
    delegate void RenderProcess();
    RenderProcess renderProcess;
    Portal[] allPortalsInScene;
    RenderTexture viewTexture;
    Matrix4x4 localToWorldMatrix;
    Matrix4x4[] recursionMatrix;
    [HideInInspector]
    MeshRenderer portalPlane;

    MeshFilter portalPlaneMeshFilter;
    bool cullPortalsNotInFrustumConst = true;
    int startRecursionIndex = 0;
    float nearClipOffset = 0.05f, nearClipLimit = 0.2f;

    void Start() 
    {
        recursionLimit = recursive ? recursionLimit : 1;
        cullPortalsNotInFrustumConst = cullPortalsNotInFrustum;
        GameObject[] portalsGo = GameObject.FindGameObjectsWithTag("Portal");
        allPortalsInScene = new Portal[portalsGo.Length];
        
        for (int i = 0; i < portalsGo.Length; i++)
            allPortalsInScene[i] = portalsGo[i].GetComponent<Portal>();

        playerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        selfCamera = GetComponentInChildren<Camera>();
        portalPlane = GetComponentInChildren<MeshRenderer>();
        portalPlaneMeshFilter = portalPlane.GetComponent<MeshFilter> ();
        portalPlane.material.SetInt ("displayMask", 1);

        CreateRenderingProcess();
    }

    void CreateRenderingProcess()
    {
        if (oneSidedPortal) renderProcess += PlayerIsInSameSideAsRender;
        renderProcess += Render;
    }

    public void RenderCall()
    {
        if (portalToRender.doNotRender) 
        {
            portalToRender.portalPlane.enabled = false;
            return;
        }
        renderProcess();
    }

    void Render()
    {
        if (!cullPortalsNotInFrustum) canBeSeen = true;

        bool renderCond = portalToRender.canBeSeen;
        if (optimizeOneSidedPortals) renderCond = portalToRender.needsToRenderPlane && portalToRender.canBeSeen;

        if (cullPortalsNotInFrustum)
        {
            if (renderCond && portalsCanSeeOtherPortals) CheckIfCameraSeesOtherPortal();
            if (!portalToRender.canBeSeenByOtherPortal) portalToRender.CheckCameraCanSee(portalToRender.portalPlane, playerCamera);
        }

        portalPlane.enabled = false;
        
        if (renderCond)
        {
            portalToTeleportTo.SetScreenSizeToPreventClipping (selfCamera.transform.position);
            CreateRenderTexture();
            startRecursionIndex = 0;

            HandlePortalCameraMovement();
        
            for (int i = startRecursionIndex; i < recursionLimit; i++)
            {
                selfCamera.transform.SetPositionAndRotation (recursionMatrix[i].GetColumn (3), recursionMatrix[i].rotation);
                HandleObliqueProjection();
                selfCamera.Render();
            }
        }

        if (needsToRenderPlane) portalPlane.enabled = true;
    }

    public void PostRender()
    {
        if (portalToRender.doNotRender) 
        {
            portalToRender.portalPlane.enabled = false;
            return;
        }

        bool renderCond = portalToRender.canBeSeen;
        if (optimizeOneSidedPortals) renderCond = portalToRender.needsToRenderPlane && portalToRender.canBeSeen;
        
        if (renderCond)
            SetScreenSizeToPreventClipping (playerCamera.transform.position);

        canBeSeenByOtherPortal = false;
    }

    void HandlePortalCameraMovement()
    {
        localToWorldMatrix = playerCamera.transform.localToWorldMatrix;
        recursionMatrix = new Matrix4x4[recursionLimit];
        selfCamera.projectionMatrix = playerCamera.projectionMatrix;

        for (int i = 0; i < recursionLimit; i++)
        {
            if (i > 0 && optimizeRecursivePortals) 
                if (!BoundsOverlap (portalPlaneMeshFilter, portalToRender.portalPlaneMeshFilter, selfCamera))
                    break; 
            
            startRecursionIndex = recursionLimit - i - 1;
            localToWorldMatrix = transform.localToWorldMatrix * portalToRender.transform.worldToLocalMatrix * localToWorldMatrix;
            recursionMatrix[startRecursionIndex] = localToWorldMatrix;
        }
    }

    void CreateRenderTexture()
    {
        if (selfCamera.targetTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (selfCamera.targetTexture != null) selfCamera.targetTexture.Release();
            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            selfCamera.targetTexture = viewTexture;
            portalToRender.portalPlane.material.SetTexture("_MainTex", viewTexture);
        }        
    }

    void HandleObliqueProjection()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        // https://danielilett.com/2019-12-18-tut4-3-matrix-matching/
        int dotProduct = Math.Sign (Vector3.Dot (transform.forward, transform.position - selfCamera.transform.position));

        Vector3 camWorldPos = selfCamera.worldToCameraMatrix.MultiplyPoint (transform.position);
        Vector3 normal = selfCamera.worldToCameraMatrix.MultiplyVector (transform.forward) * dotProduct;
        float camSpaceDst = -Vector3.Dot (camWorldPos, normal) + nearClipOffset;

        if (Mathf.Abs (camSpaceDst) > nearClipLimit) selfCamera.projectionMatrix = playerCamera.CalculateObliqueMatrix (new Vector4 (normal.x, normal.y, normal.z, camSpaceDst));
        else selfCamera.projectionMatrix = playerCamera.projectionMatrix;
    }

    void SetScreenSizeToPreventClipping (Vector3 viewPoint) {
        // // Learning resource:
        // // https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
        float frustumHeight = playerCamera.nearClipPlane * Mathf.Tan(playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth  = frustumHeight * playerCamera.aspect;

        float screenThickness = new Vector3 (frustumWidth, frustumHeight, playerCamera.nearClipPlane).magnitude;
        bool viewFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;

        portalPlane.transform.localScale = new Vector3 (portalPlane.transform.localScale.x, portalPlane.transform.localScale.y, screenThickness);
        portalPlane.transform.localPosition = Vector3.forward * screenThickness * ((viewFacingSameDirAsPortal) ? .5f : -.5f);
    }

    bool BoundsOverlap (MeshFilter nearObject, MeshFilter farObject, Camera camera) {

        var near = GetScreenRectFromBounds (nearObject, camera);
        var far = GetScreenRectFromBounds (farObject, camera);

        // ensure far object is indeed further away than near object
        if (far.zMax > near.zMin) 
        {
            // Doesn't overlap on x axis
            if (far.xMax < near.xMin || far.xMin > near.xMax) return false;
            
            // Doesn't overlap on y axis
            if (far.yMax < near.yMin || far.yMin > near.yMax) return false;
            
            // Overlaps
            return true;
        }

        return false;
    }

    public void CheckCameraCanSee(Renderer renderer, Camera camera)
    {
        // http://wiki.unity3d.com/index.php/IsVisibleFrom
        Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
		canBeSeen = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, renderer.bounds);
    }

    void CheckIfCameraSeesOtherPortal()
    {
        Plane[] cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(selfCamera);
        
        for (int i = 0; i < allPortalsInScene.Length; i++)
            if (!allPortalsInScene[i].canBeSeen && !allPortalsInScene[i].canBeSeenByOtherPortal && allPortalsInScene[i] != this)
            {
                allPortalsInScene[i].canBeSeenByOtherPortal = GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, allPortalsInScene[i].portalPlane.bounds);
                allPortalsInScene[i].canBeSeen = allPortalsInScene[i].canBeSeenByOtherPortal;
            }
    }

    // http://www.turiyaware.com/a-solution-to-unitys-camera-worldtoscreenpoint-causing-ui-elements-to-display-when-object-is-behind-the-camera/
    MinMax3D GetScreenRectFromBounds (MeshFilter renderer, Camera mainCamera) {
        MinMax3D minMax = new MinMax3D (float.MaxValue, float.MinValue);

        Vector3[] screenBoundsExtents = new Vector3[8];
        var localBounds = renderer.sharedMesh.bounds;
        bool anyPointIsInFrontOfCamera = false;

        for (int i = 0; i < 8; i++) 
        {
            Vector3 localSpaceCorner = localBounds.center + Vector3.Scale (localBounds.extents, cubeCornerOffsets[i]);
            Vector3 worldSpaceCorner = renderer.transform.TransformPoint (localSpaceCorner);
            Vector3 viewportSpaceCorner = mainCamera.WorldToViewportPoint (worldSpaceCorner);

            if (viewportSpaceCorner.z > 0) 
            {
                anyPointIsInFrontOfCamera = true;
            } 

            else 
            {
                // If point is behind camera, it gets flipped to the opposite side
                // So clamp to opposite edge to correct for this
                viewportSpaceCorner.x = (viewportSpaceCorner.x <= 0.5f) ? 1 : 0;
                viewportSpaceCorner.y = (viewportSpaceCorner.y <= 0.5f) ? 1 : 0;
            }

            // Update bounds with new corner point
            minMax.AddPoint (viewportSpaceCorner);
        }

        // All points are behind camera so just return empty bounds
        if (!anyPointIsInFrontOfCamera) return new MinMax3D ();

        return minMax;
    }

    void PlayerIsInSameSideAsRender()
    {
        needsToRenderPlane = SameSideAsRenderPlane(playerCamera.transform);
    }
    
    public void SetTexture(RenderTexture texture)
    {
        portalPlane.material.SetTexture("_MainTex", texture);
    }

    public bool SameSideAsRenderPlane(Transform t)
    {
        if (renderSide == RenderSide.Front) return Vector3.Dot (transform.forward, transform.position - t.position) < 0;
        else if (renderSide == RenderSide.Back) return Vector3.Dot (transform.forward, transform.position - t.position) > 0;

        return false;
    }

    // junk to fix clipping with optimization ### fix?
    void OnTriggerEnter(Collider other)
    {
        portalToTeleportTo.cullPortalsNotInFrustum = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (cullPortalsNotInFrustumConst) 
        {
            cullPortalsNotInFrustum = true;
            portalToTeleportTo.cullPortalsNotInFrustum = true;
        }
    }

    public struct MinMax3D 
    {
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public float zMin;
        public float zMax;

        public MinMax3D (float min, float max) 
        {
            this.xMin = min;
            this.xMax = max;
            this.yMin = min;
            this.yMax = max;
            this.zMin = min;
            this.zMax = max;
        }

        public void AddPoint (Vector3 point) 
        {
            xMin = Mathf.Min (xMin, point.x);
            xMax = Mathf.Max (xMax, point.x);
            yMin = Mathf.Min (yMin, point.y);
            yMax = Mathf.Max (yMax, point.y);
            zMin = Mathf.Min (zMin, point.z);
            zMax = Mathf.Max (zMax, point.z);
        }
    }
}
