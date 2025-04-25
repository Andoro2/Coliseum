using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SteamAchievementsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //[Button]
    public void IsUnlocked(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        Debug.Log($"Achievement {id} status: " + ach.State);
    }
    //[Button]
    public void Unlock(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Trigger();

        Debug.Log($"Achievement {id} unlocked");
    }
    //[Button]
    public void LockAchievement(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();

        Debug.Log($"Achievement {id} locked");
    }
}
