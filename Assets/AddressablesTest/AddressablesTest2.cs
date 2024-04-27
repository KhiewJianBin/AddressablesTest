using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesTest2 : MonoBehaviour
{
    AsyncOperationHandle<GameObject> player_prefab_handle;
    [SerializeField] AssetReferenceGameObject player_prefab;

    [SerializeField] GameObject loadScreen;

    GameObject playersPrefab;
    List<GameObject> players = new List<GameObject>();

    IEnumerator Start()
    {
        //Load Required Assets
        yield return GetPlayerAsset();
    }

    IEnumerator GetPlayerAsset()
    {
        loadScreen.SetActive(true);
        yield return new WaitForSeconds(1);

        player_prefab_handle = Addressables.LoadAssetAsync<GameObject>(player_prefab);
        yield return player_prefab_handle;

        if (player_prefab_handle.Status == AsyncOperationStatus.Succeeded)
        {
            playersPrefab = player_prefab_handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load Asset {player_prefab.RuntimeKey}");
            playersPrefab = null;
        }

        loadScreen.SetActive(false);
    }

    bool UpdatingCatalog = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ReturnPlayerAsset();
            StartCoroutine(GetPlayerAsset());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (!UpdatingCatalog) UpdateCatalogs();
        }

        void SpawnPlayer()
        {
            if (playersPrefab)
            {
                Instantiate(playersPrefab, transform);
            }
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

    void ReturnPlayerAsset()
    {
        //IMPORTANT NOTE, As of now, Addresables.Release takes a while to work in the background, and you cant call related addressables code else will error (maybe because while bundle is releasing)
        Addressables.Release(player_prefab_handle);
    }

    void OnDestroy()
    {
        ReturnPlayerAsset();
    }
}
