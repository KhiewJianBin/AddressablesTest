using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesTest : MonoBehaviour
{
    [SerializeField] AssetReferenceGameObject player_Prefab;
    [SerializeField] GameObject loadScreen;

    
    List<GameObject> players = new List<GameObject>();

    bool UpdatingCatalog = false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ClearPlayers();

            SpawnPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if(!UpdatingCatalog) UpdateCatalogs();
        }
    }

    async void UpdateCatalogs()
    {
        Debug.Log($"Checking for Asset Catalog Updates");
        UpdatingCatalog = true;

        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        var catalogsToUpdate = await checkForUpdateHandle.Task;

        if (catalogsToUpdate.Count == 0)
        {
            Debug.Log($"No Catalog To Update");
        }
        if (catalogsToUpdate.Count > 0)
        {
            Debug.Log($"Catalog To Update");
            catalogsToUpdate.ForEach((s) => Debug.Log(s));

            AsyncOperationHandle<List<IResourceLocator>> updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            var updates = await updateHandle.Task;

            Debug.Log($"Catalog Updated");
            updates.ForEach((s) => Debug.Log(s));
        }
        
        UpdatingCatalog = false;
    }

    async void SpawnPlayer()
    {
        if (player_Prefab.Asset == null)
        {
            loadScreen.SetActive(true);

            Debug.Log($"Attempting to load Asset {player_Prefab.RuntimeKey}");

            var handle = player_Prefab.LoadAssetAsync<GameObject>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"Failed to load Asset {player_Prefab.RuntimeKey}");
            }

            loadScreen.SetActive(false);
        }
        else
        {
            var newplayer = Instantiate(player_Prefab.Asset, transform) as GameObject;

            players.Add(newplayer);
        }
    }

    void ClearPlayers()
    {
        foreach (var player in players)
        {
            Destroy(player);
        }

        if (player_Prefab.Asset) player_Prefab.ReleaseAsset();
    }

    void OnDestroy()
    {
        if (player_Prefab.Asset) player_Prefab.ReleaseAsset();
    }
}
