using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Vurus noktasinda yukari suzulup kaybolan hasar sayisi.
/// Prefab gerektirmez, tamamen kod uzerinden olusturulur.
/// </summary>
public class DamagePopup : MonoBehaviour
{
    private const float Lifetime = 0.7f;
    private const float RiseDistance = 0.9f;

    public static void Spawn(Vector3 worldPosition, int amount, Color color)
    {
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            return;
        }

        GameObject host = new GameObject("DamagePopup", typeof(RectTransform));
        host.transform.position = worldPosition + Vector3.up * 0.35f;

        TextMeshPro label = host.AddComponent<TextMeshPro>();
        label.font = font;
        label.text = amount.ToString();
        label.color = color;
        label.fontSize = 4f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(4f, 2f);

        // Sprite'larin uzerinde ciziisin.
        Renderer renderer = host.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 500;
        }

        host.AddComponent<DamagePopup>().Begin(label);
    }

    private void Begin(TextMeshPro label)
    {
        StartCoroutine(Animate(label));
    }

    private IEnumerator Animate(TextMeshPro label)
    {
        Vector3 start = transform.position;
        Color baseColor = label.color;
        float elapsed = 0f;

        while (elapsed < Lifetime)
        {
            // Hitstop sirasinda da hareket etsin.
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / Lifetime);

            transform.position = start + Vector3.up * (RiseDistance * k);
            transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.85f, k);

            if (label != null)
            {
                label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - (k * k));
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
