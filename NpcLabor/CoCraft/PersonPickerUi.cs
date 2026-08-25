using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NpcLabor.CoCraft;

/// <summary>
/// Shared person-row styling for LayerList pickers (co-craft + processor + dispatch).
/// Vanilla people lists use ItemGeneral.SetChara → Chara.SetImage on the row icon.
/// LayerList.SetList2 always DisableIcon first; we re-enable for real characters.
/// Full-body SetImage + SetNativeSize overflows LayerList a/b/c key letters — clamp and
/// park the icon to the right of the shortcut column.
/// </summary>
internal static class PersonPickerUi
{
    // Keep under the LayerList a/b/c keyText column so shortcuts stay readable.
    // Resident-board feel: compact portrait, never cover a/b/c key letters.
    const float IconSlot = 22f;
    const float IconOffsetX = 34f;
    const float TextAfterIconX = 64f;

    internal static void StyleRow(
        ItemGeneral item,
        int id,
        string label,
        string sub,
        int menuSelf = int.MinValue,
        int menuAuto = int.MinValue,
        int menuOff = int.MinValue)
    {
        if (item == null)
        {
            return;
        }

        Chara? portrait = ResolvePortrait(id, menuSelf);
        if (portrait != null)
        {
            try
            {
                // Sets sprite on button1.icon and a default name/color.
                item.SetChara(portrait);
            }
            catch
            {
                try
                {
                    if (item.button1?.icon != null)
                    {
                        portrait.SetImage(item.button1.icon);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

            EnableIcon(item);
        }
        else
        {
            try
            {
                item.DisableIcon();
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

        // Always apply our own label after SetChara overwrites the name.
        try
        {
            if (item.button1?.mainText != null)
            {
                FontColor color = FontColor.ButtonGeneral;
                if (id == menuSelf)
                {
                    color = FontColor.Good;
                }
                else if (id == menuOff)
                {
                    color = FontColor.Bad;
                }
                else if (id == menuAuto)
                {
                    color = FontColor.Default;
                }
                else if (portrait != null && portrait.IsPCParty)
                {
                    color = FontColor.Good;
                }

                item.button1.mainText.SetText(label ?? "", color);
            }
        }
        catch
        {
            try
            {
                if (item.button1?.mainText != null)
                {
                    item.button1.mainText.text = label ?? "";
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

        try
        {
            item.Build();
        }
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
// Long picker labels (e.g. 爱好·队 + name) scroll right instead of wrapping to a second line.
        try
        {
            if (item.button1?.mainText != null)
            {
                item.button1.mainText.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
// Build / SetChara may re-expand the sprite; keep the small slot.
        if (portrait != null)
        {
            FitIconSlot(item);
        }

        if (!string.IsNullOrEmpty(sub))
        {
            try
            {
                // Do NOT call ItemGeneral.SetSubText: it runs lang.lang() and drops free-form Chinese.
                if (item.button1?.subText != null)
                {
                    item.button1.subText.SetActive(enable: true);
                    try
                    {
                        item.button1.subText.SetText(sub, FontColor.Default);
                    }
                    catch
                    {
                        item.button1.subText.text = sub;
                    }

                    item.button1.subText.alignment = TextAnchor.MiddleRight;
                    try
                    {
                        item.button1.subText.rectTransform.anchoredPosition = new Vector2(210f, 0f);
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
try
                    {
                        if (item.button1.mainText != null)
                        {
                            RectTransform mrt = item.button1.mainText.rectTransform;
                            mrt.sizeDelta = new Vector2(Mathf.Min(mrt.sizeDelta.x <= 0 ? 170f : mrt.sizeDelta.x, 170f), mrt.sizeDelta.y);
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

        // Final pass: SetSubText / Build can reshuffle transforms.
        if (portrait != null)
        {
            FitIconSlot(item);
        }
    }

    static Chara? ResolvePortrait(int id, int menuSelf)
    {
        if (id == menuSelf)
        {
            return EClass.pc;
        }

        // Special menu sentinels (auto/off/…) have no portrait.
        if (id <= 0)
        {
            return null;
        }

        Chara? c = RefChara.Get(id);
        if (c == null || c.IsPC || c == EClass.pc)
        {
            return null;
        }

        return c;
    }

    static void EnableIcon(ItemGeneral item)
    {
        try
        {
            if (item.button1?.icon == null)
            {
                return;
            }

            // DisableIcon turns off icon.transform.parent; reverse that.
            Transform parent = item.button1.icon.transform.parent;
            if (parent != null)
            {
                parent.gameObject.SetActive(true);
            }

            item.button1.icon.gameObject.SetActive(true);
            item.button1.icon.enabled = true;

            FitIconSlot(item);

            // SetList2 calls DisableIcon first (mainText x=20 when no keyText).
            // With keyText present, leave letter free and start name after portrait.
            if (item.button1.mainText != null)
            {
                RectTransform rt = item.button1.mainText.rectTransform;
                float x = item.button1.keyText != null ? TextAfterIconX : TextAfterIconX - 8f;
                rt.anchoredPosition = new Vector2(x, rt.anchoredPosition.y);
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

    /// <summary>
    /// Clamp full-body SetImage sprites into a compact slot and park them to the
    /// right of LayerList a/b/c key letters so shortcuts stay readable.
    /// </summary>
    static void FitIconSlot(ItemGeneral item)
    {
        try
        {
            Image? icon = item?.button1?.icon;
            if (icon == null)
            {
                return;
            }

            icon.preserveAspect = true;
            icon.raycastTarget = false;

            RectTransform rt = icon.rectTransform;
            // SetImage changes pivot for full-body sprites; that makes even a small
            // sizeDelta spill left over the a/b/c key letter. Center the slot.
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(IconSlot, IconSlot);
            rt.anchoredPosition = new Vector2(IconOffsetX + IconSlot * 0.5f, 0f);

            // Nudge past keyText (a/b/c) which sits on the far left of the row.
            try
            {
                Vector2 pos = rt.anchoredPosition;
                if (pos.x < IconOffsetX)
                {
                    rt.anchoredPosition = new Vector2(IconOffsetX, pos.y);
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
// Avoid layout drivers re-expanding to native texture size.
            try
            {
                var fitter = icon.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.enabled = false;
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
try
            {
                // Kill any leftover native scale from SetImage pivots.
                icon.transform.localScale = Vector3.one;
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
// Parent mask/frame: shrink oversized frames only.
            try
            {
                Transform parent = icon.transform.parent;
                if (parent != null && parent != item!.button1!.transform)
                {
                    RectTransform? prt = parent as RectTransform;
                    if (prt != null)
                    {
                        Vector2 ps = prt.sizeDelta;
                        if (ps.x > IconSlot + 8f || ps.y > IconSlot + 8f || ps.x <= 0f || ps.y <= 0f)
                        {
                            prt.sizeDelta = new Vector2(IconSlot + 4f, IconSlot + 4f);
                        }

                        Vector2 ppos = prt.anchoredPosition;
                        if (ppos.x < IconOffsetX - 4f)
                        {
                            prt.anchoredPosition = new Vector2(IconOffsetX - 4f, ppos.y);
                        }
                    }
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
// Keep shortcut letter above the portrait sprite in draw order.
            try
            {
                UIText? key = item?.button1?.keyText;
                if (key != null)
                {
                    key.gameObject.SetActive(true);
                    key.enabled = true;
                    key.transform.SetAsLastSibling();
                    try
                    {
                        RectTransform krt = key.rectTransform;
                        // Park letter on the far left; portrait starts at IconOffsetX.
                        if (krt.anchoredPosition.x > 18f)
                        {
                            krt.anchoredPosition = new Vector2(8f, krt.anchoredPosition.y);
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
try
                    {
                        CanvasGroup? cg = key.GetComponent<CanvasGroup>();
                        if (cg != null)
                        {
                            cg.alpha = 1f;
                            cg.blocksRaycasts = false;
                        }
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
try
                    {
                        // Ensure letter draws on top of any oversized sprite bleed.
                        var c = key.color;
                        c.a = 1f;
                        key.color = c;
                    }
                    catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

                // Icon under keyText.
                icon.transform.SetAsFirstSibling();
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
}

    /// <summary>True if this chara must never appear as a worker/assistant option.</summary>
    internal static bool IsPcLike(Chara? c)
    {
        if (c == null)
        {
            return true;
        }

        if (c.IsPC || c == EClass.pc)
        {
            return true;
        }

        try
        {
            if (EClass.pc != null && c.uid == EClass.pc.uid)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
        return false;
    }

    /// <summary>
    /// Unique residents who cannot join the party (Demitas etc.) stay off
    /// co-craft, processor, town labor, and dispatch pickers.
    /// </summary>
    internal static bool IsStayHomeUnique(Chara? c)
    {
        if (c == null)
        {
            return false;
        }

        try
        {
            if (c.trait is TraitChara tc && tc.IsUnique && !tc.CanJoinParty)
            {
                return true;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }

        return false;
    }

    /// <summary>
    /// Co-craft/processor-style hobby/work designator for picker rows:
    /// 爱好·队伍 艾琳 when a hobby matches, 工作·队伍 艾琳 when only an assigned
    /// work matches, else plain 队伍 艾琳. Truncates the name harder when tagged
    /// so the prefix fits on the row. partyTag is an already-localized label
    /// (队伍 / Party / 隊列) appended only for party members.
    /// </summary>
    internal static string HobbyTaggedLabel(
        Chara c,
        IReadOnlyCollection<int> skillIds,
        string partyTag,
        string name)
    {
        HobbyWorkKind kind = HobbyTag.GetRelevantKind(c, skillIds);

        int maxName = kind == HobbyWorkKind.None ? 8 : 5;
        if (name.Length > maxName)
        {
            name = name.Substring(0, maxName);
        }

        string tag = "";
        if (!string.IsNullOrEmpty(partyTag))
        {
            try
            {
                if (c.IsPCParty || (EClass.pc?.party?.members?.Contains(c) ?? false))
                {
                    tag = partyTag + " ";
                }
            }
            catch (System.Exception __e) { Plugin.LogDebug("PersonPickerUi.cs silent catch: " + __e.Message); }
        }

        if (kind == HobbyWorkKind.None)
        {
            return tag + name;
        }

        string prefix = kind == HobbyWorkKind.Hobby
            ? NpcLabor.LaborText.T("co.prefix.hobby")
            : NpcLabor.LaborText.T("co.prefix.work");
        return prefix + "·" + tag + name;
    }
}