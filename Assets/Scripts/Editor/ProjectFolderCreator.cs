#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ProjectFolderCreator
{
    [MenuItem("Tools/Project/Create Default Folders")]
    public static void CreateDefaultFolders()
    {
        string[] paths = new string[]
        {
            "Assets/Scripts",
            "Assets/Scripts/UI",
            "Assets/Scripts/Core",
            "Assets/ScriptableObject",
            "Assets/Prefabs",
            "Assets/Prefabs/UI",
            "Assets/Prefabs/Enemies",
            "Assets/Arts",//Images, Models
            "Assets/Audios",
            "Assets/Materials",
        };

        int created = 0;
        foreach (string path in paths)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                created++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"[ProjectFolderCreator] Created {created} folders (idempotent).");
    }
}
#endif