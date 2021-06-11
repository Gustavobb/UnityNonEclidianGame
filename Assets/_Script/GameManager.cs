using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public static string pastLevelName, currentLevelName;

    public static void ChangeScene(string nextLevel)
    {
        pastLevelName = currentLevelName;
        currentLevelName = nextLevel;
    }
}
