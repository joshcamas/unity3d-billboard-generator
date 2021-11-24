using UnityEngine;
using System.Collections.Generic;

namespace Ardenfall.Utilities
{
    [CreateAssetMenu(menuName = "Ardenfall/Foliage/Billboard Asset")]
    public class BillboardAsset : ScriptableObject
    {
        public GameObject prefab;
        public BillboardRenderSettings renderSettings;

        [Header("Values")]
        public int textureSize = 512;
        public float cutoff = 0.15f;

        [Header("LODs")]
        public bool pickLastLOD = true;
        public int LODIndex = 0;

        [HideInInspector]
        public List<Texture2D> generatedTextures;

        [HideInInspector]
        public Mesh generatedMesh;

        [HideInInspector]
        public Material generatedMaterial;
        
        [HideInInspector]
        public GameObject generatedPrefab;
    }
}