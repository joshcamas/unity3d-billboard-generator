using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ardenfall.Utilities
{
    public static class BillboardGeneratorUtility
    {
        private const int PREVIEW_LAYER = 22;

        public static void GenerateBillboard(BillboardAsset asset)
        {
            //Destroy textures
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));

            foreach (var loadedAsset in allAssets)
            {
                if (loadedAsset is Texture2D)
                    GameObject.DestroyImmediate(loadedAsset, true);
            }

            AssetDatabase.SaveAssets();

            var settings = asset.renderSettings;
            var bakedPassTextures = GenerateBakePasses(asset);

            //Save textures
            for (int i = 0; i < bakedPassTextures.Count; i++)
                AssetDatabase.AddObjectToAsset(bakedPassTextures[i], AssetDatabase.GetAssetPath(asset));

            AssetDatabase.SaveAssets();

            //Modify saved texture settings for the generated atlases
            for(int i = 0; i < settings.billboardTextures.Count; i++)
            {
                var textureSettings = settings.billboardTextures[i];

                ModifyTextureImporter(bakedPassTextures[i], (importer) =>
                {
                    importer.mipmapEnabled = true;
                    importer.isReadable = false;
                    importer.npotScale = textureSettings.powerOfTwo ? TextureImporterNPOTScale.ToNearest : TextureImporterNPOTScale.None;
                    importer.alphaIsTransparency = textureSettings.alphaIsTransparency;
                    importer.alphaSource = textureSettings.alphaIsTransparency ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                    return importer;
                }, false);
            }

            GetObjectVisual(asset, out Mesh prefabMesh, out Material prefabMaterial);

            //Create billboard material
            Material billboardMaterial = null;
            
            if(asset.generatedMaterial == null)
            {
                billboardMaterial = new Material(settings.billboardShader);
                billboardMaterial.name = "Generated Material";
            }
            else
            {
                billboardMaterial = asset.generatedMaterial;
                billboardMaterial.shader = settings.billboardShader;
            }

            billboardMaterial.CopyPropertiesFromMaterial(prefabMaterial);
            billboardMaterial.SetFloat("_Cutoff", settings.billboardCutoff);

            for (int i = 0; i < bakedPassTextures.Count; i++)
                billboardMaterial.SetTexture(settings.billboardTextures[i].textureId, bakedPassTextures[i]);

            //Save material
            if (asset.generatedMaterial == null)
            {
                asset.generatedMaterial = billboardMaterial;
                AssetDatabase.AddObjectToAsset(billboardMaterial, AssetDatabase.GetAssetPath(asset));
            }
            else
                EditorUtility.SetDirty(asset.generatedMaterial);

            //Create mesh
            var generatedMesh = BillboardGeneratorUtility.GenerateBillboardMesh(asset, asset.generatedMesh);
            generatedMesh.name = "Generated Mesh";

            //Save mesh
            if(asset.generatedMesh == null)
            {
                asset.generatedMesh = generatedMesh;
                AssetDatabase.AddObjectToAsset(generatedMesh, AssetDatabase.GetAssetPath(asset));
            }
            else
                EditorUtility.SetDirty(asset.generatedMesh);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public static Mesh GenerateBillboardMesh(BillboardAsset asset, Mesh existingMesh = null)
        {
            if (!GetObjectVisual(asset, out Mesh prefabMesh, out Material prefabMaterial))
                return null;

            var extents = prefabMesh.bounds.extents;
            var center = prefabMesh.bounds.center;

            float widthComparison = prefabMesh.bounds.size.x / (prefabMesh.bounds.size.x + prefabMesh.bounds.size.z);

            Mesh mesh = existingMesh == null ? new Mesh() : existingMesh;

            //TODO: Fix offset 
            mesh.vertices = new Vector3[] {
                //Back
                 new Vector3(-extents.x + center.x, -extents.y + center.y, 0),
                 new Vector3(extents.x + center.x, -extents.y + center.y, 0),
                 new Vector3(extents.x + center.x, extents.y + center.y, 0),
                 new Vector3(-extents.x + center.x, extents.y + center.y, 0),

                 //Right
                 new Vector3(0, -extents.y + center.y, -extents.z + center.z),
                 new Vector3(0, -extents.y + center.y, extents.z + center.z),
                 new Vector3(0, extents.y + center.y, extents.z + center.z),
                 new Vector3(0, extents.y + center.y, -extents.z + center.z),

                 //Forward
                 new Vector3(-extents.x + center.x, -extents.y + center.y, 0),
                 new Vector3(extents.x + center.x, -extents.y + center.y, 0),
                 new Vector3(extents.x + center.x, extents.y + center.y, 0),
                 new Vector3(-extents.x + center.x, extents.y + center.y, 0),

                 //Left
                 new Vector3(0, -extents.y + center.y, -extents.z + center.z),
                 new Vector3(0, -extents.y + center.y, extents.z + center.z),
                 new Vector3(0, extents.y + center.y, extents.z + center.z),
                 new Vector3(0, extents.y + center.y, -extents.z + center.z)
            };

            mesh.uv = new Vector2[] {
                //Back
                 new Vector2 (0, 0),
                 new Vector2 (widthComparison, 0),
                 new Vector2(widthComparison, 0.5f),
                 new Vector2 (0, 0.5f),
                 
                //Right
                 new Vector2 (widthComparison, 0),
                 new Vector2 (1, 0),
                 new Vector2(1, 0.5f),
                 new Vector2 (widthComparison, 0.5f),

                 //Forward
                 new Vector2 (widthComparison, 0.5f),
                 new Vector2 (0, 0.5f),
                 new Vector2 (0, 1),
                 new Vector2(widthComparison, 1),
                 
                //Right
                 new Vector2 (1, 0.5f),
                 new Vector2 (widthComparison, 0.5f),
                 new Vector2 (widthComparison, 1),
                 new Vector2(1, 1),
             };

            mesh.triangles = new int[] { 
                0, 1, 2, 0, 2, 3,
                4, 5, 6, 4, 6, 7,
                11, 10, 8, 10, 9, 8,
                15, 14, 12, 14, 13, 12
            };

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            return mesh;
        }

        public static Vector2Int GetAtlasSize(BillboardAsset asset, int textureHeight)
        {
            if (!GetObjectVisual(asset, out Mesh prefabMesh, out Material prefabMaterial))
                return Vector2Int.zero;

            var bounds = prefabMesh.bounds;

            float heightSize = bounds.size.y;

            int textureWidthA = Mathf.RoundToInt((float)textureHeight * bounds.size.x / heightSize);
            int textureWidthB = Mathf.RoundToInt((float)textureHeight * bounds.size.z / heightSize);

            return new Vector2Int(textureWidthA + textureWidthB, textureHeight * 2);
        }

        public static Texture2D RenderAtlas(BillboardAsset asset, int textureHeight, Shader overrideShader = null, MaterialOverrides renderOverrides = null)
        {
            if (!GetObjectVisual(asset, out Mesh prefabMesh, out Material prefabMaterial))
                return null;

            //Create object
            GameObject prefabInstance = new GameObject("Temp Object");
            prefabInstance.transform.position = Vector3.zero;
            prefabInstance.layer = PREVIEW_LAYER;

            var meshRenderer = prefabInstance.AddComponent<MeshRenderer>();

            if (overrideShader != null)
            {
                meshRenderer.sharedMaterial = new Material(overrideShader);
                meshRenderer.sharedMaterial.CopyPropertiesFromMaterial(prefabMaterial);
            }
            else
                meshRenderer.sharedMaterial = new Material(prefabMaterial);

            if (renderOverrides != null)
                renderOverrides.OverrideMaterial(meshRenderer.sharedMaterial);

            var meshFilter = prefabInstance.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = prefabMesh;

            var camera = SetupCamera();
            var bounds = prefabMesh.bounds;

            Texture2D[] textures = new Texture2D[4];

            for(int i = 0; i < 4; i++)
            {
                Vector3 direction = i == 0 ? Vector3.forward : (i == 1 ? Vector3.left : (i == 2 ? Vector3.back : Vector3.right));
                float dirSize = i == 0 || i == 2 ? bounds.size.x : bounds.size.z;
                float heightSize = bounds.size.y;
                float aspect = dirSize / heightSize;
                int textureWidth = Mathf.RoundToInt((float)textureHeight * aspect);

                UpdateCameraPosition(prefabMesh.bounds, camera, direction, (float)textureWidth / textureHeight);
                var newTexture = Render(camera, textureWidth, textureHeight, TextureFormat.RGBA32);
                
                textures[i] = newTexture;
            }

            //Cleanup
            GameObject.DestroyImmediate(camera.gameObject);
            GameObject.DestroyImmediate(prefabInstance);

            //Stitch
            Texture2D atlasTexture = new Texture2D(textures[0].width + textures[1].width, textureHeight * 2, TextureFormat.RGBA32, false);

            atlasTexture.SetPixels(0, 0, textures[0].width, textures[0].height, textures[0].GetPixels());
            atlasTexture.SetPixels(textures[0].width, 0, textures[1].width, textures[1].height, textures[1].GetPixels());
            atlasTexture.SetPixels(0, textures[0].height, textures[2].width, textures[2].height, textures[2].GetPixels());
            atlasTexture.SetPixels(textures[0].width, textures[0].height, textures[3].width, textures[3].height, textures[3].GetPixels());
            atlasTexture.Apply();

            GameObject.DestroyImmediate(textures[0]);
            GameObject.DestroyImmediate(textures[1]);
            GameObject.DestroyImmediate(textures[2]);
            GameObject.DestroyImmediate(textures[3]);

            return atlasTexture;
        }

        private static List<Texture2D> GenerateBakePasses(BillboardAsset asset)
        {
            var settings = asset.renderSettings;

            List<Texture2D> bakedPassTextures = new List<Texture2D>();

            foreach (var texture in settings.billboardTextures)
            {
                var format = texture.GetFormat();

                Vector2Int size = BillboardGeneratorUtility.GetAtlasSize(asset, Mathf.FloorToInt(settings.textureSize / 2));
                Texture2D textureAtlas = new Texture2D(size.x, size.y, format, false);
                var textureAtlasPixels = textureAtlas.GetPixels();

                foreach (var pass in texture.bakePasses)
                {
                    var generatedTexture = BillboardGeneratorUtility.RenderAtlas(asset, Mathf.FloorToInt(settings.textureSize / 2), pass.customShader, pass.materialOverrides);
                    var pixels = generatedTexture.GetPixels();

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        var pixel = pixels[i];

                        if (pass.r)
                            textureAtlasPixels[i].r = pixel.r;

                        if (pass.g)
                            textureAtlasPixels[i].g = pixel.g;

                        if (pass.b)
                            textureAtlasPixels[i].b = pixel.b;

                        if (pass.a)
                            textureAtlasPixels[i].a = pixel.a;
                    }
                }

                textureAtlas.SetPixels(textureAtlasPixels);
                textureAtlas.Apply();
                textureAtlas.name = $"Generated Texture - {texture.textureId}";

                bakedPassTextures.Add(textureAtlas);
            }

            return bakedPassTextures;
        }

        private static Camera SetupCamera()
        {
            var camera = new GameObject("ModelPreviewGeneratorCamera").AddComponent<Camera>();
            //camera.enabled = false;
            camera.nearClipPlane = 0.01f;
            camera.cullingMask = 1 << PREVIEW_LAYER;
            //camera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.orthographic = true;
            camera.allowMSAA = false;

            var additionalData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (additionalData == null)
                additionalData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

            additionalData.antialiasing = AntialiasingMode.None;
            additionalData.renderPostProcessing = false;
            additionalData.renderShadows = false;

            return camera;
        }

        private  static void UpdateCameraPosition(Bounds bounds, Camera camera, Vector3 direction, float aspect)
        {
            camera.aspect = aspect;
            camera.transform.position = bounds.center;
            camera.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            Vector4 minMax = new Vector4(Mathf.Infinity, Mathf.Infinity, Mathf.NegativeInfinity, Mathf.NegativeInfinity);
            float padding = 0;

            Vector3 point = bounds.center + bounds.extents;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.x -= bounds.size.x;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.y -= bounds.size.y;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.x += bounds.size.x;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.z -= bounds.size.z;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.x -= bounds.size.x;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.y += bounds.size.y;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);
            point.x += bounds.size.x;
            minMax = ProjectBoundingBoxMinMax(minMax, point, camera);

            float distance = bounds.extents.magnitude + 1f;
            camera.orthographicSize = (1f + padding * 2f) * Mathf.Max(minMax.w - minMax.y, (minMax.z - minMax.x) / aspect) * 0.5f;

            camera.transform.position = bounds.center - direction * distance;
            camera.farClipPlane = distance * 4f;
        }

        private static Texture2D Render(Camera camera, int width, int height, TextureFormat format)
        {
            RenderTexture temp = RenderTexture.active;
            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 16);
            RenderTexture.active = renderTex;
            GL.Clear(false, true, Vector4.zero);

            float aspect = (float)width / height;
            camera.aspect = aspect;

            camera.targetTexture = renderTex;
            camera.Render();

            camera.targetTexture = null;

            Texture2D result = new Texture2D(width, height, format, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            result.Apply(false, false);

            RenderTexture.active = temp;
            RenderTexture.ReleaseTemporary(renderTex);

            return result;
        }

        private static void ModifyTextureImporter(Texture2D texture, Func<TextureImporter, TextureImporter> modify, bool refresh)
        {
            if (null == texture) return;

            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter = modify(tImporter);
                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }

        private static bool GetObjectVisual(BillboardAsset asset, out Mesh mesh, out Material material)
        {
            mesh = null;
            material = null;

            //Grab last lod in group, if it exists
            var lodGroup = asset.prefab.GetComponentInChildren<LODGroup>();

            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();

                //Note: only supports one renderer, for sake of simplicity
                if (lods.Length == 0 || lods[lods.Length - 1].renderers.Length == 0)
                {
                    Debug.LogError("Could not find renderer on LOD");
                    return false;
                }

                var lodRenderer = lods[asset.pickLastLOD ? lods.Length - 1 - asset.LODIndex : asset.LODIndex].renderers[0];
                var lodMeshFilter = lodRenderer.gameObject.GetComponent<MeshFilter>();

                if (lodMeshFilter == null)
                {
                    Debug.LogError("Could not find meshFilter");
                    return false;
                }

                material = lodRenderer.sharedMaterial;
                mesh = lodMeshFilter.sharedMesh;
                return true;
            }

            //Otherwise just grab renderer / filter
            var meshRenderer = asset.prefab.GetComponentInChildren<MeshRenderer>();

            if (meshRenderer == null)
            {
                Debug.LogError("Could not find meshRenderer");
                return false;
            }

            if (meshRenderer.sharedMaterial == null)
            {
                Debug.LogError("Could not find meshRenderer's sharedMaterial");
                return false;
            }

            var meshFilter = asset.prefab.GetComponentInChildren<MeshFilter>();

            if (meshFilter == null)
            {
                Debug.LogError("Could not find meshFilter");
                return false;
            }

            if (meshFilter.sharedMesh == null)
            {
                Debug.LogError("Could not find meshFilter's sharedMesh");
                return false;
            }

            material = meshRenderer.sharedMaterial;
            mesh = meshFilter.sharedMesh;
            return true;
        }

        private static Vector4 ProjectBoundingBoxMinMax(Vector4 current, Vector3 point, Camera camera)
        {
            Vector3 localPoint = camera.transform.InverseTransformPoint(point);
            if (localPoint.x < current.x)
                current.x = localPoint.x;
            if (localPoint.x > current.z)
                current.z = localPoint.x;
            if (localPoint.y < current.y)
                current.y = localPoint.y;
            if (localPoint.y > current.w)
                current.w = localPoint.y;

            return current;
        }

    }
}