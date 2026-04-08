using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageTextElement : MonoBehaviour
{
    public List<DamageType> m_Elementos = new List<DamageType>();
    private WorldElements m_DmgElement;
    public TextMeshPro m_TextMeshPro;
    public Image m_Image;

    public void GetDamageInfo(WorldElements element, float dmg)
    {
        m_TextMeshPro.text = dmg.ToString();

        foreach(DamageType type in m_Elementos)
        {
            if (type.m_DmgElement == element)
            {
                m_TextMeshPro.color = type.m_ElementColor;
                if(type.m_DmgElement != WorldElements.Null) m_Image.sprite = type.m_ElementIcon;
                else m_Image.gameObject.SetActive(false);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_TextMeshPro = GetComponentInChildren<TextMeshPro>();
        m_Image = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [System.Serializable]
    public class DamageType
    {
        public WorldElements m_DmgElement;
        public Color m_ElementColor;
        public Sprite m_ElementIcon;
    }
}
