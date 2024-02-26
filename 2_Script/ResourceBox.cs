using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBox : MonoBehaviour
{
    private readonly List<object> resources = new List<object>();

    private readonly List<ResourceManager.LoadAction> loadedAction = new List<ResourceManager.LoadAction>();

    public bool IsUnLoad { private get; set; } = true;

    private void Start()
    {
        ResourceManager.UnLoadEvent += Remove;
    }

    private void OnDestroy()
    {
        if (IsUnLoad)
        {
            UnLoad();
        }
        else
        {
            Cancel();
        }

        ResourceManager.UnLoadEvent -= Remove;
    }

    public void LoadCDN<T>(string cdnFolderName, string fileName, Action<T> complete, bool isLoadingCancel = false) where T : class
    {
        LoadCDN(cdnFolderName, cdnFolderName, fileName, complete, isLoadingCancel);
    }

    public void LoadCDN<T>(string localFolderName, string cdnFolderName, string fileName, Action<T> complete, bool isLoadingCancel = false) where T : class
    {
        var path = CDNLoadUtility.LegacyPathConvert(localFolderName, cdnFolderName, fileName);

        if (isLoadingCancel) Cancel();

        ResourceManager.LoadAction action = null;
        action = ResourceManager.LoadCDN
        (
            path.localFolderName,
            path.cdnFolderName,
            path.fileName,
            complete,
            (T resource) =>
            {
                if (action != null)
                {
                    loadedAction.Remove(action);
                }

                resources.Add(resource);
            }
        );

        if (action != null)
        {
            loadedAction.Add(action);
        }
    }

    public void LoadWeb<T>(string url, Action<T> complete, bool isLoadingCancel = false) where T : class
    {
        if (isLoadingCancel) Cancel();

        ResourceManager.LoadAction action = null;
        action = ResourceManager.LoadWeb
        (
            url,
            complete,
            (T resource) =>
            {
                if(action != null)
                {
                    loadedAction.Remove(action);
                }
                
                resources.Add(resource);
            }
        );

        if (action != null)
        {
            loadedAction.Add(action);
        }
    }

    public void LoadResource<T>(string path, Action<T> complete, bool isLoadingCancel = false) where T : class
    {
        if (isLoadingCancel) Cancel();

        ResourceManager.LoadResource(path, (T resource) =>
        {
            resources.Add(resource);
            complete(resource);
        });
    }

    public T LoadResource<T>(string path, bool isLoadingCancel = false) where T : class
    {
        if (isLoadingCancel) Cancel();

        T resource = null;

        ResourceManager.LoadResource(path, (T loadResource) =>
        {
            resources.Add(loadResource);
            resource = loadResource;
        });

        return resource;
    }

    public void LoadStreaming<T>(string path, Action<T> complete, bool isLoadingCancel = false) where T : class
    {
        if (isLoadingCancel) Cancel();

        ResourceManager.LoadAction action = null;
        action = ResourceManager.LoadStreaming
        (
            path,
            complete,
            (T resource) =>
            {
                if (action != null)
                {
                    loadedAction.Remove(action);
                }

                resources.Add(resource);
            }
        );

        if (action != null)
        {
            loadedAction.Add(action);
        }
    }

    public void UnLoad(bool isLoadingCancel = true)
    {
        if (isLoadingCancel) Cancel();

        List<object> temporaryResource = new List<object>(resources);

        foreach (var resource in temporaryResource)
        {
            ResourceManager.UnLoad(resource);
        }

        resources.Clear();
    }

    private void Cancel()
    {
        foreach (var action in loadedAction)
        {
            action.Cancel();
        }

        loadedAction.Clear();
    }

    private void Remove(object resource)
    {
        resources.Remove(resource);
    }

    public static ResourceBox Make(GameObject target)
    {
        return target.AddComponent<ResourceBox>();
    }
}