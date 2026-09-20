using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PhotoHandHUD : MonoBehaviour
{
    VisualElement card;
    VisualElement image;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        card = root.Q<VisualElement>("photo-card");
        image = root.Q<VisualElement>("photo-image");

        HidePhoto();
    }

    public void ShowPhoto(RenderTexture texture)
    {
        if (card == null)
            return;

        image.style.backgroundImage = Background.FromRenderTexture(texture);
        card.style.display = DisplayStyle.Flex;
    }

    public void HidePhoto()
    {
        if (card == null)
            return;

        card.style.display = DisplayStyle.None;
    }
}
