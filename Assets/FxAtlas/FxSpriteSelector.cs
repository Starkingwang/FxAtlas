using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FxAtlas
{
    [RequireComponent(typeof(Renderer))]
    public class FxSpriteSelector : MonoBehaviour
    {
        [SerializeField]
        private FxAtlas m_Atlas;
        [SerializeField]
        private FxSprite m_Sprite;
        [SerializeField]
        private string m_Property;

        public FxAtlas Atlas { get => m_Atlas; set => m_Atlas = value; }
        public FxSprite SpriteID { get => m_Sprite; set => m_Sprite = value; }
        public string Property
        {
            get
            {
                return string.IsNullOrEmpty(m_Property) ? "_MainTex" : m_Property;
            }

            set => m_Property = value;
        }

        private int _propertyID;
        public int PropertyID
        {
            get
            {
                if (Application.isPlaying)
                {
                    return _propertyID;
                }

                return Shader.PropertyToID(Property);
            }
        }

        private int _propertySTID;
        public int PropertySTID
        {
            get
            {
                if (Application.isPlaying)
                {
                    return _propertySTID;
                }

                return Shader.PropertyToID(Property + "_ST");
            }
        }

        private Renderer _renderer = null;
        public Renderer Renderer
        {
            get
            {
                if (Application.isPlaying)
                {
                    return _renderer ?? (_renderer = GetComponent<Renderer>());
                }

                return GetComponent<Renderer>();
            }
        }

        private void Awake()
        {
            _propertyID = Shader.PropertyToID(Property);
            _propertySTID = Shader.PropertyToID(Property + "_ST");
            _renderer = GetComponent<Renderer>();

            SetSprite(SpriteID);
        }

        public void SetSprite(FxSprite sprite)
        {
            if (!Atlas || !Renderer.sharedMaterial)
            {
                return;
            }

            Vector2 scale = Vector2.one;
            Vector2 offset = Vector2.zero;
            for (int i = 0, c = Atlas.Sprites.Length; i < c; i++)
            {
                if (Atlas.Sprites[i].id == sprite.id)
                {
                    scale.x = Atlas.Sprites[i].scaleOffset.x;
                    scale.y = Atlas.Sprites[i].scaleOffset.y;
                    offset.x = Atlas.Sprites[i].scaleOffset.z;
                    offset.y = Atlas.Sprites[i].scaleOffset.w;
                    SpriteID = Atlas.Sprites[i];
                    break;
                }
            }

            var mpb = new MaterialPropertyBlock();
            Renderer.GetPropertyBlock(mpb);
            mpb.SetTexture(PropertyID, Atlas.AtlasTexture);
            mpb.SetVector(PropertySTID, new Vector4(scale.x, scale.y, offset.x, offset.y));
            Renderer.SetPropertyBlock(mpb);
            mpb.Clear();
        }
    }
}

