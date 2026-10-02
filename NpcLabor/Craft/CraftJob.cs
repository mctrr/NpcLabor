using System;
using Newtonsoft.Json;

namespace NpcLabor.Craft;

/// <summary>How much of a thing a resident is told to make.</summary>
internal enum CraftMode
{
    /// <summary>Make exactly N, then stop and report back.</summary>
    Count = 0,

    /// <summary>Top the stock up to N and keep it there — the colony pantry mode.</summary>
    Keep = 1,

    /// <summary>Never stop: make one after another while the pantry feeds it.</summary>
    Endless = 2,
}

/// <summary>
/// One standing instruction: "resident X keeps making recipe Y".
///
/// The instruction survives save/load, so the colony keeps cooking while the player is
/// away. Nothing about the work in progress is stored beyond a minute counter, which
/// makes a reload land in a sensible state whatever happened to the resident meanwhile.
/// </summary>
internal sealed class CraftJob
{
    [JsonProperty]
    public int uid;

    [JsonProperty]
    public string name = "";

    [JsonProperty]
    public string recipeId = "";

    [JsonProperty]
    public CraftMode mode = CraftMode.Endless;

    /// <summary>Meaning of the target depends on the mode; ignored for Endless.</summary>
    [JsonProperty]
    public int target = 1;

    [JsonProperty]
    public int produced;

    /// <summary>Minutes of work banked towards the next item.</summary>
    [JsonProperty]
    public double minutes;

    /// <summary>Short, player-facing reason the job is not moving right now.</summary>
    [JsonProperty]
    public string note = "";

    /// <summary>True when the last tick could not do any work (missing materials, no bench).</summary>
    [JsonProperty]
    public bool stalled;

    /// <summary>True once the instruction has been satisfied and should not run again.</summary>
    [JsonProperty]
    public bool done;

    [JsonIgnore]
    internal string RecipeLabel => CraftEngine.Name(recipeId);

    /// <summary>One line for the board: who, what, and how far along.</summary>
    internal string Describe()
    {
        string state;
        switch (mode)
        {
            case CraftMode.Count:
                state = produced + "/" + Math.Max(1, target);
                break;
            case CraftMode.Keep:
                state = "keep " + Math.Max(1, target);
                break;
            default:
                state = produced + " made";
                break;
        }

        string line = name + " — " + RecipeLabel + " (" + state + ")";
        if (done)
        {
            line += " ✓";
        }
        else if (stalled && !string.IsNullOrEmpty(note))
        {
            line += " · " + note;
        }

        return line;
    }
}
