using UnityEngine;

namespace NpcLabor.CoCraft;

/// <summary>
/// Temporary co-craft state opened on craft start and always cleared on complete/cancel/death/zone.
/// Does not permanently mutate PC skills; skill hooks read Eff while active.
/// </summary>
internal static class CoCraftSession
{
    internal const float NpcSkillWeight = 0.5f;
    internal const float NpcSpPerSkill = 0.25f;
    internal const float NpcExpShare = 0.5f;

    internal static bool Active;
    internal static int NpcUid;
    internal static int ReqSkillId;
    internal static int DurationSkillId;
    internal static int NpcSkill;
    internal static int PcSkill;
    internal static int Eff;
    internal static int CostSp;
    internal static int Duration;
    internal static int BatchNum = 1;
    internal static int NpcExpGranted;
    internal static string? AssistantName;
    /// <summary>Why the last clear happened, used for player-facing finish text.</summary>
    internal static string LastClearReason = "";

    static int _npcSkillDuration;
    static int _npcSkillReq;

    internal static Chara? GetAssistant()
    {
        if (!Active || NpcUid == 0)
        {
            return null;
        }

        Chara? c = RefChara.Get(NpcUid);
        return AssistantResolver.IsValidAssistant(c) ? c : null;
    }

    static int GetNpcSkillFor(int skillId)
    {
        if (skillId == DurationSkillId && DurationSkillId != ReqSkillId)
        {
            return _npcSkillDuration;
        }

        return _npcSkillReq;
    }

    internal static bool TryGetEff(int skillId, out int eff)
    {
        eff = 0;
        if (!Active)
        {
            return false;
        }

        if (skillId != ReqSkillId && skillId != DurationSkillId)
        {
            return false;
        }

        int pc = EClass.pc != null ? EClass.pc.Evalue(skillId) : PcSkill;
        int npcPart = Mathf.FloorToInt(GetNpcSkillFor(skillId) * NpcSkillWeight);
        Element? el = EClass.pc?.elements?.GetOrCreateElement(skillId);
        if (el != null)
        {
            pc = el.Value;
        }

        eff = pc + npcPart;
        return true;
    }

    internal static bool Open(Chara assistant, Recipe? recipe, TraitCrafter? crafter)
    {
        Clear("reopen", announce: false);

        if (!AssistantResolver.IsValidAssistant(assistant) || EClass.pc == null)
        {
            return false;
        }

        int reqSkillId = 0;
        int durationSkillId = 0;

        if (recipe?.source != null)
        {
            Element req = recipe.source.GetReqSkill();
            reqSkillId = req.id;
        }

        if (crafter != null)
        {
            string alias = crafter.IDReqEle(recipe?.source);
            if (!alias.IsEmpty())
            {
                try
                {
                    durationSkillId = EClass.sources.elements.alias[alias].id;
                }
                catch
                {
                    durationSkillId = 0;
                }
            }
        }

        if (reqSkillId == 0 && durationSkillId == 0)
        {
            Plugin.LogWarn("co-craft open skipped: no skill id");
            return false;
        }

        if (reqSkillId == 0)
        {
            reqSkillId = durationSkillId;
        }

        if (durationSkillId == 0)
        {
            durationSkillId = reqSkillId;
        }

        TeleportToPc(assistant);
        // Pin the assistant beside the PC before crafting starts so they never
        // wander off mid-progress (teleport alone cancels their old AI and the
        // branch can re-assign a goal that walks away).
        HoldAssistant(assistant);

        NpcUid = assistant.uid;
        ReqSkillId = reqSkillId;
        DurationSkillId = durationSkillId;
        _npcSkillReq = assistant.Evalue(reqSkillId);
        _npcSkillDuration = assistant.Evalue(durationSkillId);
        NpcSkill = _npcSkillReq;
        Element? pcEl = EClass.pc.elements?.GetOrCreateElement(reqSkillId);
        PcSkill = pcEl != null ? pcEl.Value : EClass.pc.Evalue(reqSkillId);
        Eff = PcSkill + Mathf.FloorToInt(NpcSkill * NpcSkillWeight);
        NpcExpGranted = 0;
        AssistantName = AssistantResolver.NameOf(assistant);
        LastClearReason = "";
        Active = true;

        Msg.SayRaw(NpcLabor.LaborText.T("co.msg.started", AssistantName ?? "?"));
        Plugin.LogInfo(
            $"co-craft open: npc={AssistantName}#{NpcUid} req={reqSkillId} dur={durationSkillId} " +
            $"pc={PcSkill} npcSkill={NpcSkill} eff={Eff}");
        return true;
    }

    internal static void TeleportToPc(Chara assistant)
    {
        if (EClass.pc?.pos == null)
        {
            return;
        }

        Point dest = EClass.pc.pos;
        try
        {
            Point? near = dest.GetNearestPoint(allowBlock: false, allowChara: true, allowInstalled: true, ignoreCenter: false);
            if (near != null && near.IsValid)
            {
                dest = near;
            }
        }
        catch
        {
            // keep pc.pos
        }

        try
        {
            assistant.Teleport(dest, silent: true, force: true);
        }
        catch
        {
            try
            {
                assistant.MoveImmediate(dest, focus: false, cancelAI: true);
            }
            catch (System.Exception ex)
            {
                Plugin.LogWarn($"assistant teleport failed: {ex.Message}");
            }
        }
    }

    /// <summary>Pin the assistant in place beside the PC for the whole craft.</summary>
    static void HoldAssistant(Chara c)
    {
        if (c == null)
        {
            return;
        }

        try { c.noMove = true; } catch { }
        try
        {
            c.SetAI(new CoCraftStandAi());
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("co-craft hold assistant: " + ex.Message);
        }
    }

    /// <summary>Free the assistant's movement and restore their default idle AI.</summary>
    static void ReleaseAssistant()
    {
        if (NpcUid == 0)
        {
            return;
        }

        try
        {
            Chara? c = RefChara.Get(NpcUid);
            if (c == null || c.isDestroyed)
            {
                return;
            }

            try { c.noMove = false; } catch { }
            try
            {
                if (c.ai is CoCraftStandAi)
                {
                    c.SetNoGoal();
                }
            }
            catch { }
        }
        catch (System.Exception ex)
        {
            Plugin.LogDebug("co-craft release assistant: " + ex.Message);
        }
    }

    internal static void RememberTiming(int costSp, int duration, int num)
    {
        if (!Active)
        {
            return;
        }

        CostSp = costSp;
        Duration = duration;
        BatchNum = num > 0 ? num : 1;
    }

    internal static int AdjustCostSp(int baseSp)
    {
        if (!Active)
        {
            return baseSp;
        }

        int reduced = baseSp - Mathf.FloorToInt(NpcSkill * NpcSpPerSkill);
        return Mathf.Max(1, reduced);
    }

    internal static void AddNpcExpGranted(int amount)
    {
        if (!Active || amount <= 0)
        {
            return;
        }

        NpcExpGranted += amount;
    }

    internal static void Clear(string reason, bool announce = true)
    {
        if (!Active && NpcUid == 0)
        {
            return;
        }

        ReleaseAssistant();

        LastClearReason = reason ?? "";
        if (announce && Active)
        {
            AnnounceFinish(reason);
        }

        Plugin.LogDebug($"co-craft clear ({reason}) uid={NpcUid} expGranted={NpcExpGranted}");
        Active = false;
        NpcUid = 0;
        ReqSkillId = 0;
        DurationSkillId = 0;
        NpcSkill = 0;
        PcSkill = 0;
        Eff = 0;
        CostSp = 0;
        Duration = 0;
        BatchNum = 1;
        NpcExpGranted = 0;
        AssistantName = null;
        _npcSkillDuration = 0;
        _npcSkillReq = 0;
    }

    static void AnnounceFinish(string reason)
    {
        string name = AssistantName ?? NpcLabor.LaborText.T("co.msg.assistantFallback");
        // Quiet internal re-open / no-assistant pre-clear.
        if (reason == "reopen" || reason == "no-assistant" || reason == "plugin-destroy")
        {
            return;
        }

        if (NpcExpGranted > 0)
        {
            Msg.SayRaw(NpcLabor.LaborText.T("co.msg.gainedExp", name, NpcLabor.LaborTerms.CoCraft));
            return;
        }

        // Keep cancel/fail quiet-lean; no spreadsheet tone.
        switch (reason)
        {
            case "ai-cancel":
            case "pc-death":
            case "zone-change":
            case "layer-kill":
            case "ai-end":
            default:
                Msg.SayRaw(NpcLabor.LaborText.T("co.msg.ended", NpcLabor.LaborTerms.CoCraft));
                break;
        }
    }
}
