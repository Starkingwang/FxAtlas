using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FxAtlas
{
    public class FxAtlas : ScriptableObject
    {
        [SerializeField]
        private Texture2D m_AtlasTexture;

        [SerializeField]
        private FxSprite[] m_Sprites;

        public Texture2D AtlasTexture
        {
            get { return m_AtlasTexture; }
            set { m_AtlasTexture = value; }
        }

        public FxSprite[] Sprites
        {
            get { return m_Sprites; }
            set { m_Sprites = value; }
        }
    }

    [System.Serializable]
    public struct FxSprite
    {
        public string name;
        public int id;
        public Vector4 scaleOffset;

        public static Rect ScaleOffsetToRect(Vector4 scaleOffset)
        {
            return new Rect(scaleOffset.z, scaleOffset.w, scaleOffset.x, scaleOffset.y);
        }
    }
}


