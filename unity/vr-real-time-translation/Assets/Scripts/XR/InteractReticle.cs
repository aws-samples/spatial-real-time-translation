using UnityEngine;

public class InteractReticle : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _indicatorSprite;
    [SerializeField] private Color _color;
    [SerializeField] private Color _selectionColor;
    [SerializeField, Range(0f, 2f)] private float _scaleMultiplier = 0.015f;

    private void Start()
    {
        _indicatorSprite.transform.localScale *= _scaleMultiplier;
        SetInvalid();
    }

    public void SetValid()
    {
        _indicatorSprite.sortingOrder = 1000;
        _indicatorSprite.color = _selectionColor;
    }

    public void SetInvalid()
    {
        _indicatorSprite.sortingOrder = 0;
        _indicatorSprite.color = _color;
    }
}
