using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace Ardenfall.Utilities
{
    [CustomEditor(typeof(BillboardAsset))]
    public class BillboardAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var asset = target as BillboardAsset;

            EditorGUI.BeginDisabledGroup(asset.prefab == null || asset.renderSettings == null);

            if (GUILayout.Button("Generate Billboard"))
                BillboardGeneratorUtility.GenerateBillboard(target as BillboardAsset);

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(asset.generatedMaterial == null || asset.generatedMesh == null);

            if (GUILayout.Button("Spawn Billboard"))
            {
                GameObject billboard = new GameObject("Billboard");
                billboard.AddComponent<MeshRenderer>().sharedMaterial = asset.generatedMaterial;
                billboard.AddComponent<MeshFilter>().sharedMesh = asset.generatedMesh;
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Force Delete Generated Data"))
                DestroyChildren(target as BillboardAsset);

        }

        public static void DestroyChildren(BillboardAsset asset)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            foreach (var loadedAsset in allAssets)
            {
                if (loadedAsset != asset)
                    GameObject.DestroyImmediate(loadedAsset, true);
            }

            AssetDatabase.SaveAssets();
        }

    }
}