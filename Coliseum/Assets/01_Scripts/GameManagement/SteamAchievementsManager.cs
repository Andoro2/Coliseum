using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SteamAchievementsManager : MonoBehaviour
{
    public string ACH_ID;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //[Button]
    public void IsUnlocked()//string id)
    {
        var ach = new Steamworks.Data.Achievement(ACH_ID);
        Debug.Log($"Achievement {ACH_ID} status: " + ach.State);
    }
    //[Button]
    public void Unlock()//string id)
    {
        var ach = new Steamworks.Data.Achievement(ACH_ID);
        ach.Trigger();

        Debug.Log($"Achievement {ACH_ID} unlocked");
    }
    //[Button]
    public void LockAchievement()//string id)
    {
        var ach = new Steamworks.Data.Achievement(ACH_ID);
        ach.Clear();

        Debug.Log($"Achievement {ACH_ID} locked");
    }
}
