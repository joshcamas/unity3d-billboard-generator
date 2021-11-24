using UnityEngine;
using System.Collections.Generic;

namespace Ardenfall.Utilities
{
    [System.Serializable]
    public class MaterialOverrides
    {
        [System.Serializable]
        public class TextureProperty
        {
            public string propertyName;
            public Texture2D propertyValue;
        }

        [System.Serializable]
        public class FloatProperty
        {
            public string propertyName;
            public float propertyValue;
        }

        [System.Serializable]
        public class IntProperty
        {
            public string propertyName;
            public int propertyValue;
        }

        [System.Serializable]
        public class VectorProperty
        {
            public string propertyName;
            public Vector4 propertyValue;
        }

        [System.Serializable]
        public class ColorProperty
        {
            public string propertyName;
            public Color propertyValue;
        }

        public List<TextureProperty> textureOverrides;
        public List<FloatProperty> floatOverrides;
        public List<IntProperty> intOverrides;
        public List<VectorProperty> vectorOverrides;
        public List<ColorProperty> colorOverrides;

        public void OverrideMaterial(Material material)
        {
            foreach (var texProp in textureOverrides)
                material.SetTexture(texProp.propertyName, texProp.propertyValue);

            foreach (var floatProp in floatOverrides)
                material.SetFloat(floatProp.propertyName, floatProp.propertyValue);

            foreach (var intProp in intOverrides)
                material.SetInt(intProp.propertyName, intProp.propertyValue);

            foreach (var vectorProp in vectorOverrides)
                material.SetVector(vectorProp.propertyName, vectorProp.propertyValue);

            foreach (var colorProp in colorOverrides)
                material.SetColor(colorProp.propertyName, colorProp.propertyValue);
        }
    }



}