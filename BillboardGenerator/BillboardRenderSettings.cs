using UnityEngine;
using System.Collections.Generic;

namespace Ardenfall.Utilities
{
    [CreateAssetMenu(menuName = "Ardenfall/Foliage/Billboard Render Settings")]
    public class BillboardRenderSettings : ScriptableObject
    {
        [System.Serializable]
        public class BillboardTexture
        {
            public string textureId = "_MainTex";
            public bool powerOfTwo = true;
            public bool alphaIsTransparency = true;

            public List<BakePass> bakePasses;

            public TextureFormat GetFormat()
            {
                Vector4 activeChannels = new Vector4();

                foreach (var bakePass in bakePasses)
                {
                    if (bakePass.r)
                        activeChannels.x++;

                    if (bakePass.g)
                        activeChannels.y++;

                    if (bakePass.b)
                        activeChannels.z++;

                    if (bakePass.a)
                        activeChannels.w++;
                }

                //Detect duplicates
                if (activeChannels.x > 1 || activeChannels.y > 1 || activeChannels.z > 1 || activeChannels.w > 1)
                    Debug.LogError("Multiple bake passes in the same texture channel detected");

                if (activeChannels.w >= 1)
                    return TextureFormat.RGBA32;

                if (activeChannels.z >= 1)
                    return TextureFormat.RGB24;

                if (activeChannels.y >= 1)
                    return TextureFormat.RG16;

                return TextureFormat.R8;
            }
        }

        [System.Serializable]
        public class BakePass
        {
            public Shader customShader;
            public MaterialOverrides materialOverrides;
            public bool r = true;
            public bool g = true;
            public bool b = true;
            public bool a = true;
        }

        public List<BillboardTexture> billboardTextures;
        public Shader billboardShader;


    }
}