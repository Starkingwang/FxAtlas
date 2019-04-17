using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FxAtlas
{
    [ExecuteInEditMode]
    public class FxAtlasAssembler : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int m_AtlasSize = new Vector2Int(1024, 1024);

        [SerializeField]
        private FxAtlas m_AtlasAsset;

        [SerializeField]
        private FxSpriteSource[] m_Sprites;

        [SerializeField]
        private List<KeyName> m_InstanceIDs = new List<KeyName>();

        public int AtlasWidth
        {
            get
            {
                return m_AtlasSize.x;
            }
        }

        public int AtlasHeight
        {
            get
            {
                return m_AtlasSize.y;
            }
        }

        public Vector2Int AtlasSize
        {
            get { return m_AtlasSize; }
            set
            {
                m_AtlasSize = value;
            }
        }

        public FxAtlas AtlasAsset
        {
            get { return m_AtlasAsset; }
            set { m_AtlasAsset = value; }
        }

        public FxSpriteSource[] Sprites
        {
            get { return m_Sprites; }
            set
            {
                m_Sprites = value;
            }
        }

        public List<KeyName> InstanceIDs
        {
            get { return m_InstanceIDs; }
            set
            {
                m_InstanceIDs = value;
            }
        }

        private void OnValidate()
        {
            UpdateSprites();
        }

        public void UpdateSprites()
        {
            var sprites = GetComponentsInChildren<FxSpriteSource>();

            if (sprites == null || sprites.Length <= 0)
            {
                if (Sprites != null)
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Edit Sprite");
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                    Sprites = null;
                    InstanceIDs.Clear();
                }
                return;
            }

            var instanceIDs = InstanceIDs;

            bool needDirty = false;

            if (Sprites == null || sprites.Length != Sprites.Length)
            {
                needDirty = true;
#if UNITY_EDITOR
                UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Edit Sprite");
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }

            foreach (var s in sprites)
            {
                bool alreadyIn = false;
                for (int i = 0; i < instanceIDs.Count; i++)
                {
                    while (instanceIDs[i].id == s.ID)
                    {
                        if (instanceIDs[i].name == s.IdentityName)
                        {
                            alreadyIn = true;
                            break;
                        }

                        if (!needDirty)
                        {
                            needDirty = true;
#if UNITY_EDITOR
                            UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Edit Sprite");
                            UnityEditor.EditorUtility.SetDirty(this);
#endif
                        }

                        s.ID++;
                    }

                    if (alreadyIn)
                    {
                        break;
                    }
                }

                if (!alreadyIn)
                {

                    if (!needDirty)
                    {
                        needDirty = true;
#if UNITY_EDITOR
                        UnityEditor.Undo.RegisterFullObjectHierarchyUndo(this, "Edit Sprite");
                        UnityEditor.EditorUtility.SetDirty(this);
#endif
                    }

                    instanceIDs.Add(new KeyName() { id = s.ID, name = s.IdentityName });
                    break;
                }
            }

            if (needDirty)
            {
                InstanceIDs = instanceIDs;
                Sprites = sprites;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/FxAtlas/Create FxAtlas Assembler", false, 8000)]
        [UnityEditor.MenuItem("GameObject/Create Other/FxAltas Assembler")]
        public static void AddNewAssembler()
        {
            var assembler = new GameObject("FxAtlas Assembler").AddComponent<FxAtlasAssembler>();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(assembler.gameObject.scene);
        }
#endif
    }

    [System.Serializable]
    public struct KeyName
    {
        public int id;
        public string name;
    }
}


