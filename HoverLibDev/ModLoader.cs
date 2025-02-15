using UnityEngine;
using System.IO;
using System.Reflection;
using MelonLoader;

///ALL OF THIS IS PLACE HOLDER
namespace HoverMenu
{
    public class ModLoader : MelonMod
    {
        public static ModLoader Instance { get; private set; }

        private int totalBundlesToLoad = 0;
        private int bundlesLoaded = 0;

        public void StartAssetBundleLoading()
        {
            string gameRootDirectory = Directory.GetParent(Application.dataPath).FullName;

            string modsDirectory = Path.Combine(gameRootDirectory, "Mods");
            string assetBundleDirectory = Path.Combine(modsDirectory, "AssetBundles");


            assetBundleDirectory = assetBundleDirectory.Replace("\\", "/");

            MelonLogger.Msg($"Mods directory: {modsDirectory}");
            MelonLogger.Msg($"AssetBundles directory: {assetBundleDirectory}");

            if (Directory.Exists(assetBundleDirectory))
            {
                string[] assetBundles = Directory.GetFiles(assetBundleDirectory, "*.bundle");

                totalBundlesToLoad = assetBundles.Length;
                if (totalBundlesToLoad > 0)
                {
                    foreach (var bundlePath in assetBundles)
                    {
                        LoadAssetBundle(bundlePath);
                    }
                }
                else
                {
                    MelonLogger.Error("No AssetBundles found in AssetBundles directory!");
                }
            }
            else
            {
                MelonLogger.Error($"AssetBundles directory does not exist: {assetBundleDirectory}");
            }
        }


        private void LoadAssetBundle(string bundlePath)
        {
            MelonLogger.Msg($"Loading AssetBundle: {bundlePath}");

            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (assetBundle != null)
            {
                MelonLogger.Msg($"AssetBundle loaded successfully: {bundlePath}");

                LoadScriptsAndAssetsConcurrently(assetBundle);

                bundlesLoaded++;

                MelonLogger.Msg($"Bundles loaded: {bundlesLoaded}/{totalBundlesToLoad}");

                if (bundlesLoaded == totalBundlesToLoad)
                {
                    MelonLogger.Msg("All asset bundles loaded successfully!");
                }
            }
            else
            {
                MelonLogger.Error($"Failed to load AssetBundle from {bundlePath}");
            }
        }

        private void LoadScriptsAndAssetsConcurrently(AssetBundle assetBundle)
        {
            string[] assetNames = assetBundle.GetAllAssetNames();
            MelonLogger.Msg($"AssetBundle contains {assetNames.Length} assets.");

            foreach (var assetName in assetNames)
            {
                MelonLogger.Msg($"Processing asset: {assetName}");

                if (assetName.EndsWith(".dll"))
                {
                    LoadDllFromAssetBundle(assetBundle, assetName);
                }
                else
                {
                    LoadNonDllAssetsFromAssetBundle(assetBundle, assetName);
                }
            }
        }

        private void LoadDllFromAssetBundle(AssetBundle assetBundle, string assetName)
        {
            try
            {
                byte[] dllBytes = assetBundle.LoadAsset<TextAsset>(assetName).bytes;
                Assembly loadedAssembly = Assembly.Load(dllBytes);
                MelonLogger.Msg($"DLL loaded: {assetName}");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Failed to load DLL {assetName}: {ex.Message}");
            }
        }

        private void LoadNonDllAssetsFromAssetBundle(AssetBundle assetBundle, string assetName)
        {
            if (assetName.EndsWith(".mat"))
            {
                Material material = assetBundle.LoadAsset<Material>(assetName);
                if (material != null)
                {
                    MelonLogger.Msg($"Material loaded: {material.name}");
                }
            }
            else if (assetName.EndsWith(".png") || assetName.EndsWith(".jpg") || assetName.EndsWith(".tga"))
            {
                Texture texture = assetBundle.LoadAsset<Texture>(assetName);
                if (texture != null)
                {
                    MelonLogger.Msg($"Texture loaded: {texture.name}");
                }
            }
            else if (assetName.EndsWith(".prefab"))
            {
                GameObject prefab = assetBundle.LoadAsset<GameObject>(assetName);
                if (prefab != null)
                {
                    GameObject instance = GameObject.Instantiate(prefab);
                    MelonLogger.Msg($"Prefab loaded and instantiated: {instance.name}");
                }
            }
            else
            {
                MelonLogger.Msg($"Unknown asset type: {assetName}");
            }
        }
    }
}
