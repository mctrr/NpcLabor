using System;
using System.Collections.Generic;
using NpcLabor.Dispatch;

namespace NpcLabor.Craft;

/// <summary>
/// Base production panel (slice G). Reached from the same dispatch board as the caravan,
/// next to the trade row, because both are "tell the people at home what to do".
///
/// Pages: the assignment list, a worker picker, a paged recipe picker, and the amount
/// step. The drop-off tag is placed straight from the first page, since it is a physical
/// object in the world rather than a setting.
/// </summary>
internal static class CraftUi
{
    enum RowKind
    {
        Header,
        Info,
        Assign,
        PlaceSpot,
        ClearAll,
        Job,
        Worker,
        Recipe,
        Page,
        Mode,
        Target,
    }

    sealed class Row
    {
        public RowKind Kind;
        public string Label = "";
        public string Sub = "";
        public Chara? Worker;
        public CraftJob? Job;
        public string RecipeId = "";
        public CraftMode Mode;
        public int Target;

        /// <summary>Page step for the recipe picker's prev/next rows.</summary>
        public int Page;

        public static Row Header(string text) => new Row { Kind = RowKind.Header, Label = text };

        public static Row Info(string text) => new Row { Kind = RowKind.Info, Label = text };
    }

    /// <summary>Half-finished assignment; lives only while the wizard is open.</summary>
    sealed class Draft
    {
        public int Uid;
        public string Recipe = "";
        public CraftMode Mode = CraftMode.Endless;
        public int Target = 10;
        public int Page;
    }

    static Draft? _draft;

    /// <summary>Recipes offered per page of the picker.</summary>
    const int RecipesPerPage = 12;

    // ────────────────────────────────────────────────────────────── pages

    internal static void OpenBoard(LayerQuestBoard? board)
    {
        try
        {
            if (!DungeonDispatchUi.IsAtPcFactionHome())
            {
                Msg.Say(LaborText.T("dis.msg.baseOnly", LaborTerms.Dispatch));
                SE.Beep();
                return;
            }

            _draft = null;
            ShowMain(board);
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft UI open failed: " + ex);
        }
    }

    static void ShowMain(LayerQuestBoard? board)
    {
        var rows = new List<Row>();

        rows.Add(Row.Header(LaborText.T("craft.ui.jobs")));
        if (CraftManager.Jobs.Count == 0)
        {
            rows.Add(Row.Info(LaborText.T("craft.ui.noJobs")));
        }
        else
        {
            for (int i = 0; i < CraftManager.Jobs.Count; i++)
            {
                CraftJob job = CraftManager.Jobs[i];
                rows.Add(new Row
                {
                    Kind = RowKind.Job,
                    Job = job,
                    Label = job.Describe(),
                    Sub = LaborText.T("craft.ui.cancel"),
                });
            }
        }

        rows.Add(new Row { Kind = RowKind.Assign, Label = LaborText.T("craft.ui.assign") });

        Thing? spot = CraftManager.FindSpot();
        string spotLine = spot == null
            ? LaborText.T("craft.ui.spot") + "：" + LaborText.T("craft.ui.spotNone")
            : LaborText.T("craft.ui.spot") + "：" + SafePos(spot);
        rows.Add(Row.Info(spotLine));
        rows.Add(new Row { Kind = RowKind.PlaceSpot, Label = LaborText.T("craft.ui.placeSpot") });

        List<Thing> pantry = CraftManager.Pantry();
        rows.Add(Row.Info(LaborText.T("craft.ui.pantry") + "：" + pantry.Count.ToString()));

        if (CraftManager.Jobs.Count > 0)
        {
            rows.Add(new Row { Kind = RowKind.ClearAll, Label = LaborText.T("craft.ui.clear") });
        }

        Show(board, LaborText.T("craft.ui.title"), rows);
    }

    static void ShowWorker(LayerQuestBoard? board)
    {
        _draft ??= new Draft();

        var rows = new List<Row>();
        rows.Add(Row.Header(LaborText.T("craft.ui.pickWorker")));

        List<Chara> pool = CraftManager.Candidates();
        for (int i = 0; i < pool.Count; i++)
        {
            Chara c = pool[i];
            bool busy = CraftManager.Has(c.uid);

            rows.Add(new Row
            {
                Kind = RowKind.Worker,
                Worker = c,
                Label = (busy ? "[·] " : "[  ] ") + SafeName(c),
                Sub = LaborText.T("craft.ui.skill",
                    CraftEngine.MakerSkill(_draft.Recipe, c).ToString(),
                    CraftEngine.RequiredSkillValue(_draft.Recipe).ToString()),
            });
        }

        if (pool.Count == 0)
        {
            rows.Add(Row.Info(LaborText.T("craft.ui.noWorker")));
        }

        Show(board, LaborText.T("craft.ui.pickWorker"), rows);
    }

    static void ShowRecipe(LayerQuestBoard? board)
    {
        _draft ??= new Draft();

        List<string> ids = CraftEngine.RecipeIds();
        int pages = Math.Max(1, (ids.Count + RecipesPerPage - 1) / RecipesPerPage);
        _draft.Page = Math.Max(0, Math.Min(_draft.Page, pages - 1));

        var rows = new List<Row>();
        rows.Add(Row.Info(LaborText.T("craft.ui.page",
            (_draft.Page + 1).ToString(), pages.ToString())));

        int start = _draft.Page * RecipesPerPage;
        int end = Math.Min(ids.Count, start + RecipesPerPage);

        for (int i = start; i < end; i++)
        {
            string id = ids[i];
            rows.Add(new Row
            {
                Kind = RowKind.Recipe,
                RecipeId = id,
                Label = CraftEngine.Name(id),
                Sub = Materials(id),
            });
        }

        if (ids.Count == 0)
        {
            rows.Add(Row.Info("-"));
        }

        if (_draft.Page > 0)
        {
            rows.Add(new Row { Kind = RowKind.Page, Page = -1, Label = LaborText.T("craft.ui.prev") });
        }

        if (_draft.Page < pages - 1)
        {
            rows.Add(new Row { Kind = RowKind.Page, Page = 1, Label = LaborText.T("craft.ui.next") });
        }

        Show(board, LaborText.T("craft.ui.pickRecipe"), rows);
    }

    static void ShowMode(LayerQuestBoard? board)
    {
        _draft ??= new Draft();

        var rows = new List<Row>
        {
            new Row { Kind = RowKind.Mode, Mode = CraftMode.Count, Label = LaborText.T("craft.ui.mode.count") },
            new Row { Kind = RowKind.Mode, Mode = CraftMode.Keep, Label = LaborText.T("craft.ui.mode.keep") },
            new Row { Kind = RowKind.Mode, Mode = CraftMode.Endless, Label = LaborText.T("craft.ui.mode.endless") },
        };

        Show(board, LaborText.T("craft.ui.pickMode"), rows);
    }

    static void ShowTarget(LayerQuestBoard? board)
    {
        _draft ??= new Draft();

        int[] choices = { 1, 5, 10, 20, 50, 100, 200 };
        var rows = new List<Row>();

        for (int i = 0; i < choices.Length; i++)
        {
            rows.Add(new Row
            {
                Kind = RowKind.Target,
                Target = choices[i],
                Label = choices[i].ToString(),
            });
        }

        Show(board, LaborText.T("craft.ui.pickTarget"), rows);
    }

    // ────────────────────────────────────────────────────────────── clicks

    static void OnClick(LayerQuestBoard? board, Row r)
    {
        try
        {
            switch (r.Kind)
            {
                case RowKind.Header:
                case RowKind.Info:
                    return;

                case RowKind.Job:
                    if (r.Job != null)
                    {
                        CraftManager.Cancel(r.Job.uid);
                    }

                    ShowMain(board);
                    return;

                case RowKind.Assign:
                    _draft = new Draft();
                    ShowWorker(board);
                    return;

                case RowKind.PlaceSpot:
                    CraftManager.PlaceSpot(EClass.pc);
                    ShowMain(board);
                    return;

                case RowKind.ClearAll:
                    CraftManager.Clear();
                    ShowMain(board);
                    return;

                case RowKind.Worker:
                    if (r.Worker != null)
                    {
                        _draft ??= new Draft();
                        _draft.Uid = r.Worker.uid;
                        _draft.Page = 0;
                        ShowRecipe(board);
                    }

                    return;

                case RowKind.Recipe:
                    OnRecipeClick(board, r);
                    return;

                case RowKind.Page:
                    if (_draft != null)
                    {
                        _draft.Page = Math.Max(0, _draft.Page + r.Page);
                        ShowRecipe(board);
                    }

                    return;

                case RowKind.Mode:
                    if (_draft != null)
                    {
                        _draft.Mode = r.Mode;
                        if (r.Mode == CraftMode.Endless)
                        {
                            Finish(board);
                        }
                        else
                        {
                            ShowTarget(board);
                        }
                    }

                    return;

                case RowKind.Target:
                    if (_draft != null)
                    {
                        _draft.Target = r.Target;
                        Finish(board);
                    }

                    return;
            }
        }
        catch (Exception ex)
        {
            Plugin.LogWarn("craft UI click: " + ex.Message);
        }
    }

    static void OnRecipeClick(LayerQuestBoard? board, Row r)
    {
        if (_draft == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(r.RecipeId))
        {
            return;
        }

        _draft.Recipe = r.RecipeId;
        ShowMode(board);
    }

    static void Finish(LayerQuestBoard? board)
    {
        if (_draft == null)
        {
            ShowMain(board);
            return;
        }

        Chara? who = null;
        try
        {
            who = RefChara.Get(_draft.Uid);
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft finish: " + ex.Message);
        }

        if (who == null)
        {
            Msg.Say(LaborText.T("craft.ui.noWorker"));
            ShowMain(board);
            return;
        }

        CraftJob? job = CraftManager.Assign(who, _draft.Recipe, _draft.Mode, _draft.Target);
        if (job == null)
        {
            SE.Beep();
            ShowMain(board);
            return;
        }

        Msg.Say(LaborText.T("craft.ui.assign") + "：" + job.Describe());
        ShowMain(board);
    }

    // ────────────────────────────────────────────────────────────── list plumbing

    static void Show(LayerQuestBoard? board, string caption, List<Row> rows)
    {
        LayerList menu = EClass.ui.AddLayer<LayerList>();
        menu.SetList2(
                rows,
                r => r.Label,
                (r, item) => OnClick(board, r),
                (r, item) => StyleRow(item, r))
            .SetSize(620f);

        SetHeader(menu, caption);
    }

    static void StyleRow(ItemGeneral? item, Row r)
    {
        if (item == null)
        {
            return;
        }

        try
        {
            item.DisableIcon();
        }
        catch
        {
        }

        try
        {
            if (item.button1?.mainText != null)
            {
                FontColor color = r.Kind == RowKind.Header ? FontColor.Default : FontColor.ButtonGeneral;
                item.button1.mainText.SetText(r.Label ?? "", color);
            }
        }
        catch
        {
        }

        try
        {
            if (!string.IsNullOrEmpty(r.Sub) && item.button1?.subText != null)
            {
                item.button1.subText.SetText(r.Sub);
            }
        }
        catch
        {
        }

        try
        {
            item.Build();
        }
        catch
        {
        }
    }

    static void SetHeader(LayerList menu, string caption)
    {
        try
        {
            menu.SetHeader(caption);
            return;
        }
        catch
        {
        }

        try
        {
            if (menu.windows != null && menu.windows.Count > 0)
            {
                menu.windows[0].SetCaption(caption);
            }
        }
        catch
        {
        }
    }

    // ────────────────────────────────────────────────────────────── text helpers

    /// <summary>One line of ingredient slots, or a note that nothing is needed.</summary>
    static string Materials(string recipeId)
    {
        try
        {
            List<Recipe.Ingredient> ings = CraftEngine.Ingredients(recipeId);
            if (ings.Count == 0)
            {
                return LaborText.T("craft.ui.materialOk");
            }

            var parts = new List<string>();
            for (int i = 0; i < ings.Count; i++)
            {
                parts.Add(CraftEngine.IngredientName(ings[i]) + "×" + CraftEngine.Need(ings[i]).ToString());
            }

            return LaborText.T("craft.ui.materials", string.Join("  ", parts.ToArray()));
        }
        catch (Exception ex)
        {
            Plugin.LogDebug("craft UI materials: " + ex.Message);
            return "";
        }
    }

    static string SafeName(Chara? c)
    {
        try
        {
            return c?.NameSimple ?? c?.Name ?? "?";
        }
        catch
        {
            return "?";
        }
    }

    static string SafePos(Thing? t)
    {
        try
        {
            return t == null ? "?" : (t.pos.x + "," + t.pos.z);
        }
        catch
        {
            return "?";
        }
    }
}
