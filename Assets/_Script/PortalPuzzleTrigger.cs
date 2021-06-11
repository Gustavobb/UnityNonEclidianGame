using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticipatingPortal
{
    public Portal portal;
    
    public enum ActionType {DontChange, ChangeRenderSide, ChangeRenderState};
    public ActionType actionType;
    public enum RenderState {Off, On, Switch};
    public RenderState renderState;
    public enum RenderSide {Back, Front, Switch};
    public RenderSide renderSide;
}

public class PortalPuzzleTrigger : MonoBehaviour
{
    [SerializeField]
    Portal actorPortal;
    [SerializeField]
    PortalPuzzleTrigger nextPortalEventTrigger;
    enum NextPortalEventTriggerActionType {On, Off, Switch, OnNotDisable};
    [SerializeField]
    NextPortalEventTriggerActionType nextPortalEventTriggerActionType;
    
    [SerializeField]
    public List<ParticipatingPortal> participatingPortals = new List<ParticipatingPortal>();
    bool done = false;
    GravityBody playerGravityBody;
    enum TriggerType {Area, Teleport};

    [SerializeField]
    TriggerType triggerType;
    [SerializeField]
    bool doOnce = true, teleportBoth = true, discartOneSided = false;

    void Start() 
    {
        GetComponent<BoxCollider>().isTrigger = true;

        if (nextPortalEventTrigger != null)
        {
            if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Off) nextPortalEventTrigger.gameObject.SetActive(true);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.On) nextPortalEventTrigger.gameObject.SetActive(false);
        }

        if (triggerType == TriggerType.Teleport)
        {
            playerGravityBody = GameObject.FindWithTag("Player").GetComponent<GravityBody>();
            playerGravityBody.onTravelled += OnPlayerTravelledPortal;
        }
    }

    void OnPlayerTravelledPortal()
    {
        if (!this.gameObject.activeSelf) return;
        bool canRunCond = (discartOneSided || playerGravityBody.traveller.canTeleport) && !done;
        if (canRunCond && (playerGravityBody.traveller.portal == actorPortal || (playerGravityBody.traveller.portal.portalToTeleportTo == actorPortal && teleportBoth)))
        {
            HandleParticipatingPortals();

            if (doOnce) 
            {
                playerGravityBody.onTravelled -= OnPlayerTravelledPortal;
                done = true;
            }
        }

    }

    void HandleParticipatingPortals()
    {
        for (int i = 0; i < participatingPortals.Count; i++)
        {
            if (participatingPortals[i].actionType != ParticipatingPortal.ActionType.DontChange)
            {
                if (participatingPortals[i].actionType == ParticipatingPortal.ActionType.ChangeRenderSide)
                {
                    if (participatingPortals[i].renderSide == ParticipatingPortal.RenderSide.Switch) 
                    {
                        if (participatingPortals[i].portal.renderSide == Portal.RenderSide.Back) participatingPortals[i].portal.renderSide = Portal.RenderSide.Front;
                        else participatingPortals[i].portal.renderSide = Portal.RenderSide.Back;
                    }

                    else if (participatingPortals[i].renderSide == ParticipatingPortal.RenderSide.Back) participatingPortals[i].portal.renderSide = Portal.RenderSide.Back;
                    else if (participatingPortals[i].renderSide == ParticipatingPortal.RenderSide.Front) participatingPortals[i].portal.renderSide = Portal.RenderSide.Front;
                }

                else if (participatingPortals[i].actionType == ParticipatingPortal.ActionType.ChangeRenderState)
                {
                   if (participatingPortals[i].renderState == ParticipatingPortal.RenderState.Switch) 
                   {
                       participatingPortals[i].portal.portalToRender.gameObject.SetActive(!participatingPortals[i].portal.gameObject.activeSelf);
                       participatingPortals[i].portal.gameObject.SetActive(!participatingPortals[i].portal.gameObject.activeSelf);
                   }

                    else if (participatingPortals[i].renderState == ParticipatingPortal.RenderState.On) 
                    {
                        participatingPortals[i].portal.portalToRender.gameObject.SetActive(true);
                        participatingPortals[i].portal.gameObject.SetActive(true);
                    }
                    else if (participatingPortals[i].renderState == ParticipatingPortal.RenderState.Off) 
                    {
                        participatingPortals[i].portal.portalToRender.gameObject.SetActive(false);
                        participatingPortals[i].portal.gameObject.SetActive(false);
                    }
                }
            }
        }

        MainCamera.GetPortalCameras();

        if (nextPortalEventTrigger != null)
        {
            if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Off) nextPortalEventTrigger.gameObject.SetActive(false);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.On || nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.OnNotDisable) nextPortalEventTrigger.gameObject.SetActive(true);
            else if (nextPortalEventTriggerActionType == NextPortalEventTriggerActionType.Switch)
            {
                nextPortalEventTrigger.gameObject.SetActive(!nextPortalEventTrigger.gameObject.activeSelf);
            } 
        }   
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<Collider>().tag == "Player" && !done && triggerType == TriggerType.Area)
        {
            if (doOnce) done = true;
            HandleParticipatingPortals();                
        }
    }
}
