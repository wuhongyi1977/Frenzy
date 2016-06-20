using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabApiCalls : MonoBehaviour {

	
    //Access the newest version of cloud script
    public static void PlayFabInitialize()
    {
        var cloudRequest = new GetCloudScriptUrlRequest()
        {
            Testing = false
        };

        PlayFabClientAPI.GetCloudScriptUrl(cloudRequest, (result) =>
        {
            Debug.Log("URL is set");

        },
        (error) =>
        {
            Debug.Log("Failed to retrieve Cloud Script URL");
        });
    }
}
