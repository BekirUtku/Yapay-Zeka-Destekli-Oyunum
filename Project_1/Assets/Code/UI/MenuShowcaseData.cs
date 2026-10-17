using UnityEngine;

/// <summary>
/// Ana menu vitrininde kullanilan sprite kareleri. Menu sahnesinde karakter
/// objesi olmadigi icin referanslar bu varlikta tutulur:
/// Assets/Resources/UI/MenuShowcase.asset. Kareler Inspector'dan degistirilebilir.
/// </summary>
[CreateAssetMenu(fileName = "MenuShowcase", menuName = "Oyun/Menu Vitrini")]
public class MenuShowcaseData : ScriptableObject
{
    [Header("Kahraman (oyuncu karakteri)")]
    [Tooltip("Sprite'lar varsayilan olarak saga mi bakiyor? Vitrinde kahraman saga doner.")]
    public bool heroFacesRight = true;
    public Sprite[] heroIdle;
    public Sprite[] heroAttack;
    public float heroFrameRate = 10f;
    public float attackFrameRate = 14f;

    [Header("Rakip")]
    [Tooltip("Sprite'lar varsayilan olarak saga mi bakiyor? Vitrinde rakip sola doner.")]
    public bool enemyFacesRight = false;
    public Sprite[] enemyIdle;
    public float enemyFrameRate = 4f;

    [Header("Ada (sol, orta, sag)")]
    public Sprite[] islandTop;
    public Sprite[] islandBottom;

    [Header("Susler")]
    public Sprite[] plants;
    public Sprite stone;
    public Sprite tree;

    [Header("Arka plan dag katmanlari (uzaktan yakina)")]
    public Sprite[] mountains;
}
