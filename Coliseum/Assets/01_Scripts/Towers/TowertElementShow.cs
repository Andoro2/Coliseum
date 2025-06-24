using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static TileElementAsigned;

public class TowertElementShow : MonoBehaviour
{
    public List<ElementIcon> ElementIcons = new List<ElementIcon>();
    public List<TileElements> TileElements = new List<TileElements>();
    public Image Element_1, Element_2, Element_3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckElementOnGround();


        //var validElements = TileElements.Where(e => e != TileElementAsigned.TileElements.Null).ToList();
        if (TileElements.Contains(TileElementAsigned.TileElements.Null)) TileElements.Remove(TileElementAsigned.TileElements.Null);

        switch (TileElements.Count)
        {
            case 0:
                Element_1.enabled = false;
                Element_2.enabled = false;
                Element_3.enabled = false;
                break;
            case 1: // un elemento
                Element_1.enabled = true;
                Element_2.enabled = false;
                Element_3.enabled = false;

                Element_1.sprite = ElementIcons.Find(e => e.Name == TileElements[0]).Icon;

                break;
            case 2: // dos elementos
                Element_1.enabled = false;
                Element_2.enabled = true;
                Element_3.enabled = true;

                Element_2.sprite = ElementIcons.Find(e => e.Name == TileElements[0]).Icon;
                Element_3.sprite = ElementIcons.Find(e => e.Name == TileElements[1]).Icon;
                break;
            case 3: // tres elementos
                Element_1.enabled = true;
                Element_2.enabled = true;
                Element_3.enabled = true;

                Element_1.sprite = ElementIcons.Find(e => e.Name == TileElements[0]).Icon;
                Element_2.sprite = ElementIcons.Find(e => e.Name == TileElements[1]).Icon;
                Element_3.sprite = ElementIcons.Find(e => e.Name == TileElements[2]).Icon;
                break;
        }
    }
    [System.Serializable]
    public class ElementIcon
    {
        public TileElements Name;
        public Sprite Icon;
    }

    public void CheckElementOnGround()
    {
        Collider[] m_Intersecting = Physics.OverlapBox(new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z), transform.localScale / 2f);

        TileElements.Clear();

        foreach (Collider c in m_Intersecting)
        {
            if (c.CompareTag("Constructable"))
            {
                TileElements Element = c.transform.parent.parent.GetComponent<TileElementAsigned>().TileElement;
                if (!TileElements.Contains(Element))
                {
                    TileElements.Add(Element);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(transform.position.x, transform.position.y - 0.3f, transform.position.z);
        Vector3 size = transform.localScale;
        Gizmos.DrawWireCube(center, size);
    }
}
