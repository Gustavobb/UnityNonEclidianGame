using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaLevelController : MonoBehaviour
{
    public GameObject doorCurrentLevel, doorNextLevel;
    [SerializeField]
    string levelName;
    [HideInInspector]
    public string pastLevelName;
    Door doorCurrentLevelScript, doorNextLevelLevelScript;
    bool playerEntered, done;
    enum ControllerType {LevelLoader, LevelUnloader};
    enum ColorMode {Unlit, Lit};
    [SerializeField]
    ColorMode colorMode;
    [SerializeField]
    ControllerType controllerType;
    GravityBody player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<GravityBody>();
        done = false;
        if (doorCurrentLevel != null) doorCurrentLevelScript = doorCurrentLevel.GetComponent<Door>();
        if (doorNextLevel != null) doorNextLevelLevelScript = doorNextLevel.GetComponent<Door>();
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<Collider>().tag == "Player" && !playerEntered && !done)
        {
            playerEntered = true;
            done = true;
            
            StartCoroutine(ManageLevel());
        }
    }

    IEnumerator ManageLevel()
    {
        AsyncOperation operation;
        
        if (controllerType == ControllerType.LevelLoader)
        {
            player.onTravelled = null;
            if (doorCurrentLevelScript != null) doorCurrentLevelScript.DoorAction(false);
            operation = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            while (!operation.isDone) yield return null;
            MainCamera.GetPortalCameras();

            if (colorMode == ColorMode.Lit)
            {
                RenderSettings.ambientLight = Color.white;
                RenderSettings.reflectionIntensity = 1f;
            }
            else
            {
                RenderSettings.ambientLight = Color.white * 2;
                RenderSettings.reflectionIntensity = 0f;
            }

            GameManager.ChangeScene(levelName);
            
            yield return new WaitForSeconds(.4f);
            if (doorNextLevelLevelScript != null) doorNextLevelLevelScript.DoorAction(true);
        } 
        else 
        {
            if (doorCurrentLevelScript != null) doorCurrentLevelScript.DoorAction(false);
            yield return new WaitForSeconds(.3f);
            operation = SceneManager.UnloadSceneAsync(GameManager.pastLevelName);
            while (!operation.isDone) yield return null;
            MainCamera.GetPortalCameras();
        } 
    }
}

