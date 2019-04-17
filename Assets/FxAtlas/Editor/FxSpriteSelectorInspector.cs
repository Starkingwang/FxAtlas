using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FxAtlas
{
    [CustomEditor(typeof(FxSpriteSelector))]
    public class FxSpriteSelectorInspector : Editor
    {
        GUIStyle style = new GUIStyle();
        const int textureRectWidth = 200;
        const int textureRectHeight = 200;

        List<string> Properties = new List<string>();
        int selectedPropertyID = -1;

        private void OnEnable()
        {
            var t = target as FxSpriteSelector;

            GetPropertyPopup(t);
            t.SetSprite(t.SpriteID);
        }

        public override void OnInspectorGUI()
        {
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Atlas"), new GUIContent("FxAtlas Asset"));
                if (c.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }

            var t = target as FxSpriteSelector;
            if (t.Atlas)
            {
                SelectPropertyID(t);

                var texRect = EditorGUILayout.GetControlRect(
                    GUILayout.Width(textureRectWidth), 
                    GUILayout.Height(textureRectHeight));

                GUI.DrawTexture(texRect, t.Atlas.AtlasTexture);
                DrawSelectBox(texRect, Color.white);

                var sprite = Select(texRect, t.Atlas);

                if (!sprite.Equals(default(FxSprite)))
                {
                    Undo.RecordObject(t, "Select Sprite");
                    EditorUtility.SetDirty(t);
                    t.SetSprite(sprite);
                }

                DrawSelectBox(GetSpriteRect(texRect, t.SpriteID, textureRectWidth, textureRectHeight), Color.cyan);
            }
        }

        void GetPropertyPopup(FxSpriteSelector target)
        {
            Properties.Clear();
            var r = target.GetComponent<Renderer>();
            if (r.sharedMaterial)
            {
                var shader = r.sharedMaterial.shader;
                var c = ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < c; i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Properties.Add(ShaderUtil.GetPropertyName(shader, i));
                    }
                }
            }

            selectedPropertyID = Properties.FindIndex(t => t == target.Property);
        }

        void SelectPropertyID(FxSpriteSelector target)
        {
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                selectedPropertyID = EditorGUILayout.Popup("Property", selectedPropertyID, Properties.ToArray());
                if (c.changed)
                {
                    Undo.RecordObject(target, "Change Property");
                    EditorUtility.SetDirty(target);
                    target.Property = Properties[selectedPropertyID];
                    target.SetSprite(target.SpriteID);
                }
            }
        }

        FxSprite Select(Rect rect, FxAtlas atlas)
        {
            var evt = Event.current;
            if (evt.type == EventType.MouseUp)
            {
                var norCenterPos = Rect.PointToNormalized(rect, evt.mousePosition);

                if (!new Rect(0, 0, 1, 1).Contains(norCenterPos))
                    return default;

                norCenterPos.y = 1 - norCenterPos.y;
                foreach (var s in atlas.Sprites)
                {
                    if (FxSprite.ScaleOffsetToRect(s.scaleOffset).Contains(norCenterPos))
                    {
                        return s;
                    }
                }
            }

            return default;
        }

        Rect GetSpriteRect(Rect texRect, FxSprite sprite, int widthSize, int heightSize)
        {
            var rect = texRect;
            rect.x += sprite.scaleOffset.z * widthSize;
            rect.y += (1 - sprite.scaleOffset.w - sprite.scaleOffset.y) * heightSize;
            rect.width *= sprite.scaleOffset.x;
            rect.height *= sprite.scaleOffset.y;

            return rect;
        }

        void DrawSelectBox(Rect rect, Color color)
        {
            var orgColor = GUI.color;
            GUI.color = color;
            GUI.Box(rect, "");
            GUI.color = orgColor;
        }
    }
}
