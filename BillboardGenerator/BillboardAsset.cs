using UnityEngine;
using System.Collections.Generic;

namespace Ardenfall.Utilities
{
    [CreateAssetMenu(menuName = "Ardenfall/Foliage/Billboard Asset")]
    public class BillboardAsset : ScriptableObject
    {
        public GameObject prefab;
        public BillboardRenderSettings renderSettings;

        public bool pickLastLOD = true;
        public int LODIndex = 0; 

        [HideInInspector]
        public Mesh generatedMesh;

        [HideInInspector]
        public Material generatedMaterial;
        
        [HideInInspector]
        public GameObject generatedPrefab;
    }
}