using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public RectTransform cursorImage;
    public Animator anim; 

    void Start()
    {
        UnityEngine.Cursor.visible = false;
    }
    void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorImage.parent as RectTransform, 
            Input.mousePosition, 
            null, 
            out mousePos
        );
        cursorImage.anchoredPosition = mousePos;
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Click");
            
        }
    }
}