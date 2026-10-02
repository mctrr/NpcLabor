using NpcLabor.TownLabor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NpcLabor.Dispatch;

/// <summary>
/// PARTIAL: EloMap region pin icons: draw, restore, restyle, scrub.
/// Split from DungeonDispatchManager.cs (2026-10-02).
/// </summary>
internal static partial class DungeonDispatchManager
{
    /// <summary>
    /// Pure visual overworld pins for active region dispatches.
    /// Uses the 旅行商人营地 (tinkerCamp) zone icon tile so the outing is easy to
    /// spot on EloMap. Never pass "iconFlag" as an AddLight prefab id — that is a
    /// zone tile tag and leaves null SpriteRenderers that NRE in OnChangeHour.
    /// Old elolight pins are still scrubbed/removed for save/session safety.
    /// </summary>
    internal static void RefreshRegionMapMarkers()
    {
        try
        {
            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                return;
            }

            // Restore previous pins, scrub broken lights, then re-pin as camp icons.
            ClearRegionMapMarkers();
            ScrubBrokenEloMapLights(map);

            foreach (DungeonDispatchMission m in Missions)
            {
                if (m == null || !m.isRegion)
                {
                    continue;
                }

                if (!TryAddRegionMarker(map, m.regionGx, m.regionGy))
                {
                    Plugin.LogDebug("region marker add failed @" + m.regionGx + "," + m.regionGy);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region markers: " + ex.Message);
        }
    }

    /// <summary>
    /// Defensive repair for saves/sessions that already polluted actor.lights with null sr.
    /// Safe to call every hour before vanilla EloMapActor.OnChangeHour.
    /// </summary>
    internal static void SanitizeRegionMapLights()
    {
        try
        {
            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                return;
            }

            ScrubBrokenEloMapLights(map);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    internal static void ClearRegionMapMarkers()
    {
        try
        {
            if (_pinnedRegionMarks == null)
            {
                _pinnedRegionMarks = new List<long>();
            }

            if (_pinnedRegionPrevObj == null)
            {
                _pinnedRegionPrevObj = new Dictionary<long, int>();
            }

            EloMap? map = TryGetEloMap();
            if (map == null)
            {
                _pinnedRegionMarks.Clear();
                _pinnedRegionPrevObj.Clear();
                return;
            }

            foreach (long key in _pinnedRegionMarks.ToList())
            {
                int gx = (int)(key >> 32);
                int gy = unchecked((int)(key & 0xffffffffL));
                try
                {
                    TryRestoreRegionMarker(map, gx, gy, key);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            }

            _pinnedRegionMarks.Clear();
            _pinnedRegionPrevObj.Clear();
            ScrubBrokenEloMapLights(map);
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static List<long> _pinnedRegionMarks = new List<long>();

    /// <summary>Previous EloMap cell.obj under each active region pin (restore on clear).</summary>
    static Dictionary<long, int> _pinnedRegionPrevObj = new Dictionary<long, int>();

    /// <summary>Cached tinkerCamp SourceZone icon id; 0 means unresolved.</summary>
    static int _regionDispatchIcon;

    static EloMap? TryGetEloMap()
    {
        try
        {
            return EClass.world?.region?.elomap ?? EClass.scene?.elomap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Icon tile for region-dispatch pins = 旅行商人营地 (tinkerCamp) art.
    /// Runtime-resolved from SourceZone.pos[2]; fallback 334.
    /// </summary>
    static int GetRegionDispatchIcon()
    {
        if (_regionDispatchIcon > 0)
        {
            return _regionDispatchIcon;
        }

        try
        {
            // Elin SourceDataString.map.TryGetValue(id) returns the row (not Dictionary out-bool).
            SourceZone.Row? row = null;
            try
            {
                row = EClass.sources?.zones?.map?.TryGetValue("tinkerCamp");
            }
            catch
            {
                row = null;
            }

            if (row == null)
            {
                try
                {
                    if (EClass.sources?.zones?.map != null
                        && EClass.sources.zones.map.ContainsKey("tinkerCamp"))
                    {
                        row = EClass.sources.zones.map["tinkerCamp"];
                    }
                }
                catch
                {
                    row = null;
                }
            }

            if (row?.pos != null && row.pos.Length >= 3 && row.pos[2] > 0)
            {
                _regionDispatchIcon = row.pos[2];
                return _regionDispatchIcon;
            }
        }
        catch (System.Exception __e)
        {
            Plugin.LogDebug("region icon resolve: " + __e.Message);
        }

        _regionDispatchIcon = 334;
        return _regionDispatchIcon;
    }

    static long RegionPinKey(int gx, int gy)
    {
        return ((long)gx << 32) | (uint)gy;
    }

    static bool TryAddRegionMarker(EloMap map, int gx, int gy)
    {
        if (map == null)
        {
            return false;
        }

        int icon = GetRegionDispatchIcon();
        if (icon <= 0)
        {
            return false;
        }

        EloMap.Cell? cell = null;
        try
        {
            cell = map.GetCell(gx, gy);
        }
        catch
        {
            cell = null;
        }

        if (cell == null)
        {
            return false;
        }

        // Drop any legacy light pin on this tile (pre-icon builds).
        try
        {
            TryRemoveOurLight(map, gx, gy);
        }
        catch
        {
        }

        long key = RegionPinKey(gx, gy);
        int currentObj = 0;
        try { currentObj = cell.obj; } catch { currentObj = 0; }

        // Already showing the camp pin — just track it.
        if (currentObj == icon)
        {
            if (!_pinnedRegionPrevObj.ContainsKey(key))
            {
                // Unknown prior tile; treat as empty so clear erases the pin.
                _pinnedRegionPrevObj[key] = 0;
            }

            RememberRegionPin(gx, gy);
            return true;
        }

        Zone? zone = null;
        try
        {
            zone = cell.zone;
        }
        catch
        {
            zone = null;
        }

        // Do not stomp real town/dungeon icons. Field (and empty) tiles are fair game.
        if (!CanRestyleRegionCell(zone, currentObj, icon))
        {
            Plugin.LogDebug("region marker skip real zone @" + gx + "," + gy
                + " obj=" + currentObj
                + (zone != null ? (" id=" + (zone.id ?? "?")) : ""));
            return false;
        }

        if (!_pinnedRegionPrevObj.ContainsKey(key))
        {
            _pinnedRegionPrevObj[key] = currentObj;
        }

        // Prefer restyling a bound field zone so EloMap.SetZone stays consistent.
        try
        {
            if (zone != null && IsFieldLikeZone(zone))
            {
                try { zone.icon = icon; } catch { }
                map.SetZone(gx, gy, zone, updateMesh: true);
                // SetZone no-ops when cell.obj already equals z.icon; force tile if needed.
                ForceRegionIconTile(map, cell, gx, gy, icon);
                RememberRegionPin(gx, gy);
                return true;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region marker SetZone: " + ex.Message);
        }

        // Direct objmap overlay when no restylable field zone is bound.
        try
        {
            ForceRegionIconTile(map, cell, gx, gy, icon);
            RememberRegionPin(gx, gy);
            return true;
        }
        catch (Exception ex)
        {
            ScrubBrokenEloMapLights(map);
            Plugin.LogDebug("region marker tile: " + ex.Message);
            return false;
        }
    }

    static void TryRestoreRegionMarker(EloMap map, int gx, int gy, long key)
    {
        if (map == null)
        {
            return;
        }

        // Always drop legacy light pins at this coordinate.
        try { TryRemoveOurLight(map, gx, gy); } catch { }

        int prevObj = 0;
        bool hadPrev = false;
        if (_pinnedRegionPrevObj != null && _pinnedRegionPrevObj.TryGetValue(key, out int stored))
        {
            prevObj = stored;
            hadPrev = true;
        }

        EloMap.Cell? cell = null;
        try { cell = map.GetCell(gx, gy); } catch { cell = null; }
        if (cell == null)
        {
            return;
        }

        int icon = GetRegionDispatchIcon();
        int currentObj = 0;
        try { currentObj = cell.obj; } catch { currentObj = 0; }

        // Only restore tiles we actually restyled (or that still show our pin).
        if (!hadPrev && currentObj != icon)
        {
            return;
        }

        Zone? zone = null;
        try { zone = cell.zone; } catch { zone = null; }

        if (zone != null && IsFieldLikeZone(zone))
        {
            try
            {
                // Put field icon back; 0 erases the overworld tile via SetZone.
                zone.icon = prevObj;
                map.SetZone(gx, gy, zone, updateMesh: true);
            }
            catch (Exception ex)
            {
                Plugin.LogDebug("region marker restore SetZone: " + ex.Message);
            }
        }

        // Force tile back even if SetZone no-op'd (cell.obj already matched).
        try
        {
            if (prevObj <= 0)
            {
                cell.obj = 0;
                if (map.objmap != null)
                {
                    map.objmap.Erase(gx, gy);
                    map.objmap.UpdateMeshImmediate();
                }
            }
            else
            {
                ForceRegionIconTile(map, cell, gx, gy, prevObj);
            }
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("region marker restore tile: " + ex.Message);
        }
    }

    static void ForceRegionIconTile(EloMap map, EloMap.Cell cell, int gx, int gy, int icon)
    {
        if (map == null || cell == null)
        {
            return;
        }

        try { cell.obj = icon; } catch { }

        try
        {
            if (map.objmap == null)
            {
                return;
            }

            if (icon <= 0)
            {
                map.objmap.Erase(gx, gy);
            }
            else
            {
                map.objmap.SetTile(gx, gy, icon);
            }

            map.objmap.UpdateMeshImmediate();
        }
        catch
        {
        }
    }

    static bool CanRestyleRegionCell(Zone? zone, int currentObj, int pinIcon)
    {
        if (currentObj == 0 || currentObj == pinIcon)
        {
            return true;
        }

        if (zone == null)
        {
            // Bare tile with some decorative obj — still allow pin so the outing is visible.
            return true;
        }

        return IsFieldLikeZone(zone);
    }

    static bool IsFieldLikeZone(Zone zone)
    {
        if (zone == null)
        {
            return false;
        }

        try
        {
            if (zone is Region)
            {
                return false;
            }
        }
        catch
        {
        }

        string id = "";
        try { id = zone.id ?? ""; } catch { id = ""; }

        if (string.Equals(id, "field", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Transient region outing fields sometimes keep a synthetic id like "region:plain".
        if (id.StartsWith("region:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    static void TryRemoveOurLight(EloMap map, int gx, int gy)
    {
        if (map == null)
        {
            return;
        }

        EloMapActor? actor = null;
        try
        {
            actor = map.actor;
        }
        catch
        {
            return;
        }

        if (actor?.lights == null)
        {
            return;
        }

        try
        {
            for (int i = actor.lights.Count - 1; i >= 0; i--)
            {
                EloMapLight light = actor.lights[i];
                if (light == null)
                {
                    try { actor.lights.RemoveAt(i); } catch { }
                    continue;
                }

                if (light.gx != gx || light.gy != gy)
                {
                    continue;
                }

                // Destroy only entries we own at this pin. If sr is already null, just drop the slot.
                try
                {
                    if (light.sr != null && light.sr.gameObject != null)
                    {
                        UnityEngine.Object.DestroyImmediate(light.sr.gameObject);
                    }
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                try
                {
                    actor.lights.RemoveAt(i);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
                // One pin per cell for our markers.
                break;
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static void ScrubBrokenEloMapLights(EloMap? map)
    {
        if (map == null)
        {
            return;
        }

        EloMapActor? actor = null;
        try
        {
            actor = map.actor;
        }
        catch
        {
            return;
        }

        if (actor?.lights == null || actor.lights.Count == 0)
        {
            return;
        }

        try
        {
            for (int i = actor.lights.Count - 1; i >= 0; i--)
            {
                EloMapLight? light = null;
                try
                {
                    light = actor.lights[i];
                }
                catch
                {
                    continue;
                }

                bool broken = light == null;
                if (!broken && light != null)
                {
                    try
                    {
                        broken = light.sr == null;
                    }
                    catch
                    {
                        broken = true;
                    }
                }

                if (!broken)
                {
                    continue;
                }

                try
                {
                    actor.lights.RemoveAt(i);
                }
                catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
            }
        }
        catch (System.Exception __e) { Plugin.LogDebug("DungeonDispatchManager.cs silent catch: " + __e.Message); }
    }

    static bool IsOurRegionPin(int gx, int gy)
    {
        if (_pinnedRegionMarks == null)
        {
            return false;
        }

        long key = RegionPinKey(gx, gy);
        return _pinnedRegionMarks.Contains(key);
    }

    static void RememberRegionPin(int gx, int gy)
    {
        if (_pinnedRegionMarks == null)
        {
            _pinnedRegionMarks = new List<long>();
        }

        long key = RegionPinKey(gx, gy);
        if (!_pinnedRegionMarks.Contains(key))
        {
            _pinnedRegionMarks.Add(key);
        }
    }
}
