using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour 
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Pokemon Pokemon { get; set; }

    Image Image;
    Vector3 originalPos;
    Color originalColor;


    public void Awake()

    {
        Image = GetComponent<Image>();
        if (Image == null)

        {
            Debug.LogError($"BattleUnit '{name}': no Image component found on this GameObject.");
        }
        originalPos = Image.transform.localPosition;
        originalColor = Image.color;
        // Play the enter animation when the unit awakes so it appears on screen
        PlayEnterAnimation();
    }
    public void Setup()
    {
        Pokemon = new Pokemon(_base, level);
        if (isPlayerUnit)
            Image.sprite = Pokemon.Base.BackSprite;
        else
            Image.sprite = Pokemon.Base.FrontSprite;
        Image.color = originalColor;
        PlayEnterAnimation();
    }
    public void PlayEnterAnimation()
    {   
        if (isPlayerUnit)
            Image.transform.localPosition = new Vector3(-500f, originalPos.y);
        else
            Image.transform.localPosition = new Vector3(500f, originalPos.y);

        Image.transform.DOLocalMoveX(originalPos.x, 1f);
    }
    public void PlayAttackAnimation()
    { 
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(Image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(Image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        sequence.Append(Image.transform.DOLocalMoveX(originalPos.x, 0.25f));

    }
    public void PlayHitAnimation()
    { 
        var sequence = DOTween.Sequence();
        sequence.Append(Image.DOColor(Color.red, 0.1f));
        sequence.Append(Image.DOColor(originalColor, 0.1f));
    }
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        // flash gray then restore color
        sequence.Append(Image.DOColor(Color.gray, 0.1f));
        sequence.Append(Image.DOColor(originalColor, 0.1f));
        // then move down off-screen
        sequence.Append(Image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(Image.DOFade(0f, 0.5f));
    }
}