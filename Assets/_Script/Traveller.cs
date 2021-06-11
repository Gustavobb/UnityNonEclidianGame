using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Traveller
{
    delegate void HandlePortal();
    
    HandlePortal handlePortal;
    
    public Portal portal;
    
    GravityBody gravityBody;

    Vector3 previousOffsetFromPortal;

    [HideInInspector]
    public bool canTeleport = false;

    public Traveller(GravityBody gravityBody)
    {
        this.gravityBody = gravityBody;
        this.portal = null;
    }

    void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, float scale, Camera playerCamera, Camera portalCamera) 
    {
        gravityBody.transform.position = pos;
        gravityBody.transform.rotation = rot;
        gravityBody.transform.localScale *= scale;
        gravityBody.runSpeed *= scale;
        gravityBody.gravity *= scale;
        gravityBody.jumpHeigth *= scale;
        gravityBody.speed *= scale;
        gravityBody.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (gravityBody.velocity));
        gravityBody.rb.velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (gravityBody.rb.velocity));
        playerCamera.GetComponent<OutlinePostProcess>().postprocessMaterial = portalCamera.GetComponent<OutlinePostProcess>().postprocessMaterial;
        Physics.SyncTransforms ();
        gravityBody.distanceToTheGround = gravityBody.GetComponent<Collider>().bounds.extents.y;
    }

    void HandleTeleportation()
    {
        if (!portal.gameObject.activeSelf) 
        {
            OnExitPortal();
            return;
        }
        
        Vector3 offsetFromPortal = gravityBody.transform.position - portal.transform.position;
        int sideFromPortal = Math.Sign (Vector3.Dot(offsetFromPortal, portal.transform.forward));
        int sideFromPortalOld = Math.Sign (Vector3.Dot(previousOffsetFromPortal, portal.transform.forward));
        
        if (sideFromPortal != sideFromPortalOld)
        {
            canTeleport = !(portal.oneSidedPortal && portal.SameSideAsRenderPlane(gravityBody.transform));
            if (gravityBody.onTravelled != null) gravityBody.onTravelled();
            
            if (canTeleport)
            {
                Matrix4x4 m = portal.portalToTeleportTo.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * gravityBody.transform.localToWorldMatrix;    
                float scale =  portal.portalToTeleportTo.portalScaler ?  portal.portalToTeleportTo.transform.localScale.y/portal.transform.localScale.y : 1f;
                Teleport (portal.transform, portal.portalToTeleportTo.transform, m.GetColumn(3), m.rotation, scale, portal.playerCamera, portal.portalToTeleportTo.selfCamera);
                OnExitPortal();
            }
        }
        
        previousOffsetFromPortal = offsetFromPortal;
    }

    public void LateUpdateTraveller()
    {
        if (handlePortal != null) handlePortal();
    }

    public void OnEnterPortal(Portal portalArg)
    {
        if (portal == null) 
        {
            portal = portalArg;
            previousOffsetFromPortal = gravityBody.transform.position - portal.transform.position;
            handlePortal += HandleTeleportation;
        }
    }

    public void OnExitPortal()
    {
        handlePortal -= HandleTeleportation;
        portal = null;
    }
}