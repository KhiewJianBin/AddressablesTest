using System;
using System.Collections.Generic;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesLoader
{
    static HashSet<AssetReference> currentAssetsLoading = new HashSet<AssetReference>();

    public static void LoadAssetAsync<T>(this AssetReference asset, Action<AsyncOperationHandle> OnComplete)
    {
        if (currentAssetsLoading.Contains(asset)) return;

        AsyncOperationHandle handle = asset.LoadAssetAsync<T>();
        handle.Completed += OnComplete;
        handle.Completed += (_) => { currentAssetsLoading.Remove(asset); };

        currentAssetsLoading.Add(asset);
    }
}