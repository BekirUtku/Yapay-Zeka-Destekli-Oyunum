using System.Collections;
using UnityEngine;

/// <summary>Panellerin acilis/kapanis animasyonlari. Olceklenmemis zaman kullanir.</summary>
public static class UIAnim
{
    /// <summary>
    /// Arka plani (group) soldurur, karti (card) hafifce buyutup yerine oturtur.
    /// Kapanista group objesi devre disi birakilir.
    /// </summary>
    public static IEnumerator Show(CanvasGroup group, RectTransform card, bool show, float duration = 0.22f)
    {
        if (show)
        {
            group.gameObject.SetActive(true);
        }
        group.blocksRaycasts = show;
        group.interactable = show;

        float fromAlpha = group.alpha;
        float toAlpha = show ? 1f : 0f;
        float fromScale = show ? 0.96f : 1f;
        float toScale = show ? 1f : 0.98f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = EaseOutCubic(Mathf.Clamp01(elapsed / duration));

            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
            if (card != null)
            {
                card.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, k);
            }
            yield return null;
        }

        group.alpha = toAlpha;
        if (card != null)
        {
            card.localScale = Vector3.one * toScale;
        }

        if (!show)
        {
            if (card != null)
            {
                card.localScale = Vector3.one;
            }
            group.gameObject.SetActive(false);
        }
    }

    public static float EaseOutCubic(float t)
    {
        float u = 1f - t;
        return 1f - u * u * u;
    }
}
