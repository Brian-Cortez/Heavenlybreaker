using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using static System.IO.Directory;
using static System.IO.Path;
using static UnityEditor.AssetDatabase;

public static class Setup
{
    [MenuItem("Tools/Setup/Import Essential Assets")]
    static void ImportEssentials()
    {
        Assets.ImportAsset("Odin Inspector and Serializer.unitypackage", "Sirenix/Editor ExtensionsSystem");
        Assets.ImportAsset("Odin Validator.unitypackage", "Sirenix/Editor ExtensionsUtilities");
        Assets.ImportAsset("Editor Console Pro.unitypackage", "FlyingWorm/Editor ExtensionsSystem");
        Assets.ImportAsset("Better Hierarchy.unitypackage", "Toaster Head/Editor ExtensionsUtilities");
        Assets.ImportAsset("Hot Reload Edit Code Without Compiling.unitypackage", "The Naughty Cult/Editor ExtensionsUtilities");
        Assets.ImportAsset("PrimeTween High-Performance Animations and Sequences.unitypackage", "Kyrylo Kuzyk/Editor ExtensionsAnimation");
        Assets.ImportAsset("vFavorites 2.unitypackage", "kubacho lab/Editor ExtensionsUtilities");
        Assets.ImportAsset("Wingman - Your Inspectors Best Friend.unitypackage", "Kyle Rhoads/Editor ExtensionsUtilities");
        
    }

    [MenuItem("Tools/Setup/Install Essential Packages")]
    public static void InstallPackages()
    {
        string[] packages =
        {
            "com.unity.cinemachine",
            "git+https://github.com/KyleBanks/scene-ref-attribute.git",
            "git+https://github.com/adammyhre/Unity-Improved-Timers.git",
            "git+https://github.com/adammyhre/Unity-Utils.git"
        };
        Packages.InstallPackages(packages);
    }
    
    [MenuItem("Tools/Setup/Create Default Folders")]
    public static void CreateDefaultFolders()
    {
        Folders.CreateDefault("_Project", "Animation", "Art", "Materials", "Prefabs", "Scripts", "Audio");
        Refresh();
        
        Folders.Move("_Project", "Scenes");
        Folders.Move("_Project", "Settings");
        Folders.Delete("TutorialInfo");
        Refresh();

        MoveAsset("Assets/InputSystem_Actions.inputactions", "Assets/_Project?Settings/InputSystem_Actions.inputactions");

        DeleteAsset("Assets/Readme.asset");
        Refresh();
    }
    static class Assets
    {
        public static void ImportAsset(string asset, string folder)
        {
            var basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var assetsFolder = Combine(basePath, "Unity/Asset Store-5.x");
            ImportPackage(Combine(assetsFolder, folder, asset), false);
        }
    }

    static class Packages
    {
        static AddRequest request;
        static Queue<string> packagesToInstall = new Queue<string>();

        static async void StartNextPackageInstallation()
        {
            request = Client.Add(packagesToInstall.Dequeue());

            while (!request.IsCompleted) await Task.Delay(10);
            
            if(request.Status == StatusCode.Success) Debug.Log("Installed: " + request.Result.packageId);
            else if(request.Status == StatusCode.Failure) Debug.LogError(request.Error.message);

            if (packagesToInstall.Count > 0)
            {
                await Task.Delay(1000);
                StartNextPackageInstallation();
            }
        }
        public static void InstallPackages(string[] packages)
        {
            foreach (var package in packages)
            {
                packagesToInstall.Enqueue(package);
            }

            if (packagesToInstall.Count > 0)
            {
                StartNextPackageInstallation();
            }
        }
    }
    
    static class Folders
    {
        public static void CreateDefault(string root, params string[] folders)
        {
            var fullPath = Combine(Application.dataPath, root);
            if (!Exists(fullPath))
            {
                CreateDirectory(fullPath);
            }

            foreach (var folder in folders)
            {
                CreateSubFolders(fullPath, folder);
            }
        }

        static void CreateSubFolders(string rootPath, string folderHierarchy)
        {
            var folders = folderHierarchy.Split('/');
            var currentPath = rootPath;

            foreach (var folder in folders)
            {
                currentPath = Combine(currentPath, folder);
                if (!Exists(currentPath))
                {
                    CreateDirectory(currentPath);
                }
            }
        }

        public static void Move(string newParent, string folderName)
        {
            var sourcePath = $"Assets/{folderName}";
            if (IsValidFolder(sourcePath))
            {
                var destinationPath = $"Assets/{newParent}/{folderName}";
                var error = MoveAsset(sourcePath, destinationPath);

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to move {folderName}: {error}");
                }
            }
        }
        public static void Delete (string folderName)
        {
            var pathToDelete = $"Assets/{folderName}";
            if (IsValidFolder(pathToDelete))
            {
                DeleteAsset(pathToDelete);
            }
        }
    }
}