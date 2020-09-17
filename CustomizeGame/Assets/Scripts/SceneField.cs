using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Makes it possible to assign a scene asset in the inspector and load the scene data in a build.
/// </summary>

[Serializable]
public class SceneField
#if UNITY_EDITOR
: ISerializationCallbackReceiver
#endif
{
    #region Parameters

    #if UNITY_EDITOR
    [SerializeField] SceneAsset sceneAsset = null;
    [SerializeField] bool logErrorIfNotInBuild = false;
    #endif

    #pragma warning disable 414
    [SerializeField] int buildIndex = 0;
    #pragma warning restore 414

    #endregion

    /// <summary>
    /// Gets the scene build index.
    /// </summary>

    public int BuildIndex
    {
        get
        {
#if UNITY_EDITOR
            {
                return GetBuildIndex(sceneAsset, logErrorIfNotInBuild);
            }
#else
            {
                return buildIndex;
            }
#endif
        }
    }

    #region ISerializationCallbackReceiver implementation
#if UNITY_EDITOR


    /// <summary>
    /// Implementation of <see cref="ISerializationCallbackReceiver.OnBeforeSerialize"/>.
    /// </summary>

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        buildIndex = GetBuildIndex(sceneAsset, logErrorIfNotInBuild);
    }

    /// <summary>
    /// Implementation of <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/>.
    /// </summary>

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
    }


    #endif
    #endregion

    #region Editor members
    #if UNITY_EDITOR


    /// <summary>
    /// ** Editor-only **
    /// Gets the scene asset, if assigned.
    /// </summary>

    public SceneAsset EditorSceneAsset => sceneAsset;

    /// <summary>
    /// ** Editor-only **
    /// Retrieves the build index.
    /// </summary>
    /// <param name="sceneAsset">The scene asset.</param>
    /// <param name="logErrorIfSceneNotInBuild">Log an error if the scene is not in builds.</param>
    /// <returns>The build index, -1 if not found.</returns>

    static int GetBuildIndex(SceneAsset sceneAsset, bool logErrorIfSceneNotInBuild)
    {
        int buildIndex;

        if (sceneAsset != null)
        {
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);

            buildIndex = -1;
            int i = -1;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    i++;
                if (scene.path == scenePath)
                {
                    buildIndex = scene.enabled ? i : -1;
                    break;
                }
            }

            if (logErrorIfSceneNotInBuild && buildIndex < 0)
                Debug.LogError("The scene \"" + scenePath + "\" is referenced by an object, but is not added to builds");
        }
        else
        {
            buildIndex = -1;
        }

        return buildIndex;
    }

    /// <summary>
    /// Implementation of MonoBehaviour.Reset().
    /// </summary>

    void Reset()
    {
        sceneAsset = null;
        logErrorIfNotInBuild = true;
    }

    #endif
    #endregion
}