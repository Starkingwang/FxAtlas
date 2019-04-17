using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FxAtlas
{
    public class FxSpriteSource : MonoBehaviour
    {
        [SerializeField]
        private string m_IdentityName;
        [SerializeField]
        private int m_ID;
        [SerializeField]
        private Rect m_PixelRect = new Rect(0, 0, 256, 256);
        [SerializeField]
        private Texture2D m_Texture;

        public string IdentityName
        {
            get
            {
                return m_IdentityName;
            }

            set
            {
                m_IdentityName = value;
                name = m_IdentityName;
            }
        }

        public int ID
        {
            get { return m_ID; }
            set
            {
                m_ID = value;
            }
        }

        public Rect PixelRect
        {
            get { return m_PixelRect; }
            set
            {
                m_PixelRect = value;
            }
        }

        public Texture2D Texture
        {
            get { return m_Texture; }
            set
            {
                m_Texture = value;
                if (string.IsNullOrEmpty(m_IdentityName))
                {
                    m_IdentityName = m_Texture ? m_Texture.name : "";
                }
                if (!string.IsNullOrEmpty(m_IdentityName))
                {
                    name = m_IdentityName;
                }
            }
        }

        //区域错误
        public bool PositionOverlap { get; set; }

        //命名重复
        public bool NameConflict { get; set; }

        public Rect GetRect(Rect atlasDrawRect, int atlasWidth, int atlasHeight)
        {
            Vector2 resize = new Vector2(atlasDrawRect.width / atlasWidth, atlasDrawRect.height / atlasHeight);

            var position = PixelRect.position;
            position.y = GetFlipY(position.y, atlasHeight, PixelRect.height);

            position = position * resize + atlasDrawRect.position;
            var size = PixelRect.size * resize;

            return new Rect(position, size);
        }

        public Rect GetPixelRectFlipY(int atlasHeight)
        {
            var rect = PixelRect;
            rect.y = GetFlipY(rect.y, atlasHeight, rect.height);
            return rect;
        }

        public void SetUVPosition(Vector2 normalizedPosition, int atlasWidth, int atlasHeight)
        {
            var pixelPos = normalizedPosition * new Vector2(atlasWidth, atlasHeight);
            pixelPos.x = Mathf.Clamp((int)(pixelPos.x / PixelRect.width), 0, atlasWidth / (int)PixelRect.width - 1);
            pixelPos.y = Mathf.Clamp((int)(pixelPos.y / PixelRect.height), 0, atlasHeight / (int)PixelRect.height - 1);
            pixelPos = pixelPos * PixelRect.size;
            pixelPos.y = GetFlipY(pixelPos.y, atlasHeight, PixelRect.height);
            PixelRect = new Rect(pixelPos, PixelRect.size);
        }

        public Vector4 GetUVRect(int atlasWidth, int atlasHeight)
        {
            return new Vector4(
                PixelRect.width / atlasWidth,
                PixelRect.height / atlasHeight,
                PixelRect.x / atlasWidth,
                PixelRect.y / atlasHeight);
        }

        private float GetFlipY(float y, float atlasHeight, float spriteHeight)
        {
            return atlasHeight - spriteHeight - y;
        }
    }
}

