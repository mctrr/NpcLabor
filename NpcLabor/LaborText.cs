using System;
using System.Collections.Generic;

namespace NpcLabor;

/// <summary>
/// Lightweight player-facing strings. CN default; EN / JP when the game language matches.
/// Keep lean tone; no spreadsheet dumps.
/// </summary>
internal static partial class LaborText
{
    static readonly Dictionary<string, string> Cn = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["term.dungeonExplore"] = "地牢探索",
        ["term.regionDispatch"] = "地区派遣",
        ["term.dispatch"] = "派遣",
        ["term.townWork"] = "店铺帮工",
        ["term.coCraft"] = "共同制造",
        ["term.assist"] = "协助",
        ["term.process"] = "加工",

        ["unlock.region"] = "{0}需据点{1}级（当前{2}级）",
        ["unlock.dungeon"] = "{0}需据点{1}级（当前{2}级）",

        ["reward.money"] = "金币×{0}",
        ["reward.plat"] = "白金币×{0}",
        ["reward.furnitureTicket"] = "家具兑换券",
        ["reward.leftover"] = "打工奖励 {0}",
        ["reward.skillDiscount"] = "技能再熟练一点，报酬会更好…",
        ["reward.investTownCap"] = "似乎城镇投资等级需要提升。。。",
        ["reward.investRaised"] = "{0}店铺更受欢迎了。。。",

        ["town.settle.success"] = "完成",
        ["town.settle.recall"] = "召回",
        ["town.settle.fail"] = "失败",
        ["town.settle.line"] = "{0} 的「{1}」{2}。",
        ["town.you"] = "你",

        ["town.ui.self"] = "1  自己做",
        ["town.ui.companion"] = "2  交给同伴",
        ["town.ui.cancel"] = "取消",
        ["town.ui.selfSub"] = "约 {0} 小时 · 耗体力",
        ["town.ui.companionSub"] = "约 {0} 小时 · 选 1 人",
        ["town.ui.skillWarn"] = "这活儿还得再练练手，报酬会更好…",
        ["town.ui.stop"] = "停下（无主奖）",
        ["town.ui.recall"] = "召回工人（无主奖）",
        ["town.ui.close"] = "关闭",
        ["town.leave.confirm"] = "你正在店铺帮工。现在离开的话，帮工仍会继续计时，确定要离开吗？",
        ["town.leave.confirmSelf"] = "你正在亲自店铺帮工。离开前请选择：",
        ["town.leave.abort"] = "1  退出打工（任务失败）",
        ["town.leave.swap"] = "2  换 NPC 来继续",
        ["town.leave.cancel"] = "取消",
        ["town.leave.header"] = "亲自帮工 · 离开",
        ["town.leave.noCandidates"] = "没有空闲同伴可接替",
        ["town.leave.swapOk"] = "{0} 接替了「{1}」。",
        ["town.leave.swapFail"] = "换人失败",
        ["town.leave.abortOk"] = "你退出了「{0}」。",

        ["dispatch.settle.success"] = "成功",
        ["dispatch.settle.fail"] = "失败",
        ["dispatch.settle.recall"] = "召回",
        ["dispatch.settle.end"] = "结束",
        ["dispatch.settle.line"] = "{0} 的{1}（{2}）{3}。",
        ["dispatch.fail.hint.dungeon"] = "若成功或许还能带回首领战利品与声望。",
        ["dispatch.fail.hint.region"] = "若成功或许还能带回更完整的收获。",

        // ---- common ----
        ["common.cancel"] = "取消",

        // ---- co-craft ----
        ["co.mode.off"] = "关闭",
        ["co.mode.auto"] = "自动",
        ["co.mode.autoName"] = "自动·{0}",
        ["co.mode.autoUnknown"] = "自动(?)",
        ["co.mode.you"] = "(你)",
        ["co.tag.party"] = "队",
        ["co.tag.home"] = "居",
        ["co.prefix.hobby"] = "爱好",
        ["co.prefix.work"] = "工作",
        ["co.menu.noAssist"] = "不协助",
        ["co.menu.noOne"] = "无人可协助",
        ["co.menu.skillShort"] = "技",
        ["co.msg.started"] = "{0} 正在和你一起制造。",
        ["co.msg.assistantFallback"] = "助手",
        ["co.msg.gainedExp"] = "{0} 在{1}中熟练了一些。",
        ["co.msg.ended"] = "{0}结束了。",
        ["co.msg.autoSet"] = "改为自动选择助手。",
        ["co.msg.off"] = "协助已关闭。",
        ["co.msg.pinned"] = "由 {0} 协助。",
        ["co.msg.pinFailed"] = "指定的助手现在不可用，已关闭协助。",

        // ---- process ----
        ["proc.mode.self"] = "自己",
        ["proc.mode.selfYou"] = "自己（你）",
        ["proc.mode.auto"] = "自动",
        ["proc.mode.autoNone"] = "自动(无)",
        ["proc.menu.selfProcess"] = "亲自加工",
        ["proc.menu.pickBest"] = "选最合适的人",
        ["proc.menu.noOne"] = "无人可用",
        ["proc.menu.skillShort"] = "技",
        ["proc.msg.selfMode"] = "由你自己加工。",
        ["proc.msg.autoSet"] = "自动：当前 {0}。放满材料后开始。",
        ["proc.msg.autoNone"] = "自动：附近暂时没有合适的人。",
        ["proc.msg.pinned"] = "交给 {0}。放满材料后开始。",
        ["proc.msg.pinFailed"] = "指定的人现在不可用，已改回自己操作。",
        ["proc.msg.busy"] = "已有加工任务在进行中。",
        ["proc.msg.materialIncomplete"] = "材料不完整。",
        ["proc.msg.noWorker"] = "没有可用的加工人员，改由你自己操作，或重新选择。",
        ["proc.msg.notEnoughMaterial"] = "材料数量不够。",
        ["proc.msg.changed"] = "材料在开始时发生了变化。",
        ["proc.msg.cannotTake"] = "无法取出材料。",
        ["proc.msg.cannotOperate"] = "{0} 不会操作这台机器。",
        ["proc.msg.start"] = "{0} 开始在{1}上加工（{2} 次）。",
        ["proc.msg.residentFallback"] = "居民",
        ["proc.msg.machineFallback"] = "加工机",
        ["proc.msg.doneSkilled"] = "{0} 完成了{1}，手更熟了。",
        ["proc.msg.done"] = "{0} 完成了{1}。",
        ["proc.msg.interrupted"] = "{0} 的{1}中断了。",
        ["proc.msg.stopped"] = "{0} 停止了{1}。",
        ["proc.msg.noFuel"] = "燃料不够，{0}停下了。",
        ["proc.msg.noMaterial"] = "材料用完了，{0}停下了。",
        ["proc.msg.noMachine"] = "机器不见了，{0}停下了。",
        ["proc.msg.stuck"] = "路被堵住了，{0}停下了。",
        ["proc.msg.exhausted"] = "{0}太累了，停下了。",
        ["proc.msg.ended"] = "{0}的{1}结束了。",
        ["proc.msg.zoneContinue"] = "{0}留在原地图继续{1}。",
        ["proc.msg.zoneResume"] = "{0}继续加工（剩余 {1} 次）。",

        // ---- craft UI ----
        ["craft.effLine"] = "{0} / 有效 {1}",
        ["craft.effLineSkill"] = "{0} {1} / 有效 {2}",

        // ---- dispatch ----
        ["dis.error.invalidTarget"] = "无效目标",
        ["dis.error.notUnlocked"] = "{0}未解锁",
        ["dis.error.tooManyTarget"] = "同一目标最多 {0} 人",
        ["dis.error.tooManyDungeon"] = "同一地牢最多 {0} 人",
        ["dis.error.cannotSend"] = "{0} 无法派出",
        ["dis.error.noOne"] = "没有可派出的角色",
        ["dis.error.regionBusy"] = "该地区已有{0}进行中",
        ["dis.error.dungeonBusy"] = "该地牢已有{0}进行中",
        ["dis.error.needBase"] = "需要据点",
        ["dis.error.regionTile"] = "附近没有可进入的该地区格子",
        ["dis.error.dungeonNo"] = "该地牢不可{0}",
        ["dis.start.region"] = "{0} 出发{1}：{2}（{3} 人 · 探索{4}周）",
        ["dis.start.dungeon"] = "{0} 出发{1}：{2}（{3} 人 · 约{4}天）",
        ["dis.error.startFail"] = "出发失败: {0}",
        ["dis.loot.none"] = "暂无收获",
        ["dis.parcel.harvest"] = "派遣收获",
        ["dis.parcel.partial"] = "派遣部分收获",
        ["dis.parcel.recall"] = "派遣召回",
        ["dis.parcel.loot"] = "派遣战利品",
        ["dis.parcel.package"] = "派遣包裹",
        ["dis.loot.item"] = "物品",
        ["dis.loot.noMission"] = "无任务",
        ["dis.loot.noPlan"] = "暂无计划收获",
        ["dis.loot.moreKinds"] = "…共{0}种",
        ["dis.loot.totalLine"] = "{0} (共{1}件 x{2})",
        ["dis.loot.etcItems"] = "{0} 等{1}项",
        ["dis.loot.planFail"] = "计划读取失败: {0}",
        ["dis.loot.special"] = "特殊",
        ["dis.debug.nonRegion"] = "非地区任务（成功时现算） 当前追踪: ",
        ["dis.debug.harvestDump"] = "派遣成功计划收获 x{0}: ",
        ["dis.debug.output"] = "已输出 {0} 个任务计划收获",
        ["dis.debug.noMission"] = "当前没有派遣任务",
        ["dis.debug.forceDone"] = "已强制完成 {0} 个派遣（成功收获已入邮箱）",
        ["dis.debug.notFound"] = "未找到派遣任务 {0}",
        ["dis.debug.forceDoneOne"] = "已强制完成派遣 #{0} {1}",
        ["dis.exp.explore"] = "探索",
        ["dis.exp.lockpick"] = "开锁",
        ["dis.exp.gather"] = "采集",
        ["dis.exp.dig"] = "挖掘",
        ["dis.exp.lumber"] = "伐木",
        ["dis.exp.tunnel"] = "挖洞",
        ["dis.exp.zero"] = "{0}+0",
        ["dis.exp.gain"] = "{0}+{1}(lv{2} 差{3})",
        ["dis.compass.e"] = "东",
        ["dis.compass.w"] = "西",
        ["dis.compass.s"] = "南",
        ["dis.compass.n"] = "北",
        ["dis.compass.near"] = "附近",
        ["dis.yield.rich"] = "丰厚",
        ["dis.yield.medium"] = "中等",
        ["dis.yield.normal"] = "普通",
        ["dis.yield.meager"] = "微薄",
        ["dis.stat.none"] = "未选择",
        ["dis.stat.line"] = "战{0} 探{1} 锁{2} 采{3}",
        ["dis.stat.lineChance"] = "战{0} 探{1} 锁{2} 采{3} {4}%",
        ["dis.region.name.plain"] = "平原",
        ["dis.region.name.forest"] = "森林",
        ["dis.region.name.beach"] = "沙滩",
        ["dis.region.name.mountain"] = "山脉",
        ["dis.region.name.area"] = "地区",
        ["dis.dist"] = "{0} 距{1}",
        ["dis.distDanger"] = "{0} 距{1} 危{2}",
        ["dis.kind.random"] = "随机",
        ["dis.kind.fixed"] = "固定",
        ["dis.kind.randomDungeon"] = "随机地牢",
        ["dis.kind.fixedDungeon"] = "固定地牢",
        ["dis.preview.region"] = "{0}\n{1}  {2}\n{3}人 · {4}\n探索{5}周\n探{6} 开锁{7} 采集{8}",
        ["dis.preview.dungeon"] = "{0}\n{1}  危险{2}  {3}\n{4}人 · {5}\n约{6}天 · 成功率{7}%\n战力{8}  探{9} 开锁{10} 采集{11}",
        ["dis.info.dungeon"] = "{0}（{1}） {2} 危险{3} · 约{5}天",
        ["dis.info.region"] = "{0}（地区） {1} · 探索{2}周",
        ["dis.msg.baseOnly"] = "只能在据点使用{0}。",
        ["dis.msg.busyTalk"] = "{0} 正在探索中，不便交谈。",
        ["dis.msg.busyInteract"] = "{0} 正在探索中，无法互动。",
        ["dis.msg.empty"] = "当前没有{0}/{1}",
        ["dis.ui.activeHeader"] = "— 进行中任务 —",
        ["dis.ui.randomHeader"] = "— {0}·随机 —",
        ["dis.ui.fixedHeader"] = "— {0}·固定 —",
        ["dis.ui.noTargets"] = "附近没有已知目标",
        ["dis.ui.recallPartial"] = "召回（部分奖励）",
        ["dis.ui.recallFail"] = "召回失败",
        ["dis.ui.progress"] = "{0}% 剩{1}天",
        ["dis.ui.taskHeader"] = "任务 · {0}",
        ["dis.ui.weeks"] = "探索{0}周",
        ["dis.ui.chooseWeeks"] = "{0} · 选择探索周数",
        ["dis.ui.noLastTeam"] = "没有可用的上次队伍",
        ["dis.ui.noAutoPick"] = "没有可自动选择的成员",
        ["dis.ui.noCandidates"] = "没有可派出的居民或队友",
        ["dis.ui.confirmSel"] = "确认（{0}/{1}）",
        ["dis.ui.lastTeam"] = "上次队伍",
        ["dis.ui.maxHarvest"] = "最大收获",
        ["dis.ui.maxCombat"] = "战斗最大收获",
        ["dis.ui.partyTag"] = "队伍",
        ["dis.ui.restoreLast"] = "恢复该目标上次队伍",
        ["dis.ui.autoPickGather"] = "按探索/采集优先选满",
        ["dis.ui.autoPickPower"] = "按战力优先选满",
        ["dis.ui.pickHeader"] = "{0} · 选择人员（可多选，最多{1}）",
        ["dis.ui.pickHeaderWeeks"] = "{0} · 探索{1}周 · 选人（最多{2}）",
        ["dis.ui.tooMany"] = "同一地城最多 {0} 人",
        ["dis.ui.pickFirst"] = "请先选择人员",
        ["dis.ui.invalidChoice"] = "选择无效",
        ["dis.ui.confirmSend"] = "确认派出",
        ["dis.ui.busySuffix"] = "（进行中）",
        ["dis.ui.inProgress"] = "进行中",
        ["dis.q.ended"] = "（已结束）",
        ["dis.q.members"] = "成员: {0}",
        ["dis.q.position"] = "位置: {0}",
        ["dis.q.progress"] = "进度 {0}%",
        ["dis.q.left"] = "剩{0}天",
        ["dis.q.harvest"] = "收获: {0}",
        ["dis.q.regionPos"] = "{0} ·{1}周",
        ["dis.q.target"] = "目标",
        ["dis.q.entrance"] = "入口",
        ["dis.q.area"] = "地区",
        ["dis.q.plan"] = "计划收获: {0}",

        // ---- town labor ----
        ["town.error.invalidJob"] = "无效岗位",
        ["town.error.noTownWork"] = "据点没有{0}",
        ["town.error.invalidPlace"] = "无效地点",
        ["town.error.busy"] = "你正忙着",
        ["town.error.busyCoCraft"] = "正在共同制造中",
        ["town.error.busyProcess"] = "正在{0}中",
        ["town.error.clientBusy"] = "该委托人已有{0}进行中",
        ["town.error.maxConcurrent"] = "本城同时最多 {0} 个{1}",
        ["town.error.jobGone"] = "委托人岗位已失效",
        ["town.error.noSp"] = "体力不足，先休息一下再打工",
        ["town.error.workerBusy"] = "工人忙碌中",
        ["town.error.cannotAssign"] = "{0} 不可派工",
        ["town.msg.youStart"] = "你前往 {0} 处做「{1}」（约 {2} 小时）。",
        ["town.msg.workerStart"] = "{0} 去 {1} 做「{2}」（约 {3} 小时）。",
        ["town.msg.youTired"] = "你太累了，停下了「{0}」。",
        ["town.msg.workStart"] = "你开始在 {0} 处做「{1}」。",
        ["town.msg.endedShort"] = "该{0}已结束",
        ["town.msg.stopFail"] = "停下失败",
        ["town.msg.recallFail"] = "召回失败",
        ["town.msg.jobInvalid"] = "岗位无效",
        ["town.msg.clientGone"] = "委托人不在",
        ["town.msg.noCandidates"] = "没有空闲的居民或队员",
        ["town.msg.invalidChoice"] = "人选无效",
        ["town.msg.town"] = "店铺",
        ["town.msg.townName"] = "城镇",
        ["town.msg.worker"] = "工人",
        ["town.msg.companion"] = "同伴",
        ["town.msg.client"] = "委托人",
        ["town.progress.arriving"] = "前往工作地点…",
        ["town.progress.line"] = "{0}%  剩余{1}小时",
        ["town.ui.confirmStart"] = "确认开工",
        ["town.ui.sendConfirm"] = "派 {0} 去做「{1}」",
        ["town.ui.hours"] = "约 {0} 小时",
        ["town.ui.choosePick"] = "{0} · 选 1 人（约 {1} 时）",
        ["town.ui.skillFallback"] = "技能",
        ["town.ui.partyTag"] = "队伍",
        ["town.ui.busyTag"] = "{0}中",
        ["town.ui.youTag"] = "· 你",
        ["town.ui.needHelp"] = "店里忙，需要帮忙。",
        ["town.ui.inProgress"] = "进行中",
        ["town.ui.header"] = "{0} · {1} · {2}",
        ["town.reward.hint"] = "报酬：约金币×{0}",
        ["town.reward.line"] = "报酬 {0}",
        ["town.reward.moneyOnly"] = "报酬：金币×{0}",
        ["town.busy.invalid"] = "无效角色",
        ["town.busy.town"] = "正在{0}中",
        ["town.busy.dungeon"] = "正在{0}中",
        ["town.busy.process"] = "正在{0}中",
        ["town.busy.coCraft"] = "正在协助制造中",
        ["town.talk.busy"] = "{0} 正在{1}中，不便交谈。",
        ["town.talk.noInteract"] = "{0} 正在{1}中，无法互动。",

        // ---- job catalog ----
        ["job.inn.title"] = "旅店杂役",
        ["job.inn.skill"] = "搬运/杂务",
        ["job.inn.r1"] = "最近城里人多，客房和厅堂都转不过来。我自己忙着招呼客人，请替我把客房收拾干净，顺便把楼下的桌面和脏布也处理一下。",
        ["job.inn.r2"] = "冒险者扎堆住店，夜里脚步声没停过。我脱不开身，请派个手脚利落的人来帮忙铺床、倒水、把走廊收拾整齐。",
        ["job.inn.r3"] = "明天有一拨常客要来，今天周转特别紧。平时杂务都是我自己做，现在实在顾不上，请替我把店里的杂活顶一阵。",
        ["job.inn.r4"] = "店里杯盘和行李堆着，前台又在排队。我腾不出手，请找人帮我把客房和公用间收拾到能再接客的程度。",
        ["job.kitchen.title"] = "厨房帮厨",
        ["job.kitchen.skill"] = "料理",
        ["job.kitchen.r1"] = "出餐高峰到了，案板上的菜还没切完。平时备菜洗碗都是我自己做，现在脱不开身，请替我在厨房搭把手。",
        ["job.kitchen.r2"] = "灶火正旺，锅碗已经堆起来了。我要盯着主菜，请派个人来洗锅、递盘、把出餐口也照看一下。",
        ["job.kitchen.r3"] = "订单比平时多，厨房转不过弯。不需要你掌勺，只要帮我把杂活顶住，别让出餐断档就行。",
        ["job.kitchen.r4"] = "临时缺帮手，汤底和配菜都压着。请找个仔细点的人来备菜洗碗，忙完这一阵就好。",
        ["job.general.title"] = "杂货店员",
        ["job.general.skill"] = "交涉",
        ["job.general.r1"] = "货架有些乱，门口还排着问价的人。我要去清点进货，请替我看一会儿柜台，把货理齐、把常客招呼好。",
        ["job.general.r2"] = "刚到的货还没上架，店里又走不开人。平时理货都是我自己做，现在脱不开身，请派个人来搭把手。",
        ["job.general.r3"] = "最近问价的人特别多，一个人顾不过来。请找个口齿清楚的人帮我看店，拿不准的货别乱报价，喊我就行。",
        ["job.general.r4"] = "店里杂务堆着，绳子蜡烛和瓶罐都要重摆。我忙着记账，请替我把货架整理好，顺带应付一下柜台。",
        ["job.smith.title"] = "铁匠助手",
        ["job.smith.skill"] = "锻造",
        ["job.smith.r1"] = "炉火正旺，订单还压着。我脱不开身，请替我拉风箱、递钳子，把打完的铁屑边角也一并收好。",
        ["job.smith.r2"] = "今天锻打排得满，一个人转不过来。不需要你掌锤，只要帮我递工具、清台面，听锤声做事就行。",
        ["job.smith.r3"] = "半成品和碎铁皮堆在砧台上。平时收拾都是我自己做，现在忙着赶活，请派个助手来搭把手。",
        ["job.smith.r4"] = "临时缺帮手，风箱和边角料都顾不上。请找个沉得住气的人来当助手，烫手的东西别乱摸。",
        ["job.food.title"] = "食品店员",
        ["job.food.skill"] = "料理/交易",
        ["job.food.r1"] = "早市货刚卸下来，台秤旁还忙着。我脱不开身，请替我把货摆整齐，称重时招呼客人。",
        ["job.food.r2"] = "买的人比平时多，一个人看不过来柜台。请派个人来帮忙摆摊、称重，卖相差的先放到后面。",
        ["job.food.r3"] = "新鲜货要趁早卖，筐里果菜还没码好。平时都是我自己做，现在腾不出手，请找人搭把手。",
        ["job.food.r4"] = "柜台忙，进货也压着。请替我把食品摆上前，笑着招呼来买的人，忙过这阵就行。",
        ["job.book.title"] = "书店店员",
        ["job.book.skill"] = "阅读",
        ["job.book.r1"] = "我缺了整理书架的人手。平时都是自己分类编号，但我现在脱不开身，请替我把新到的书按架归位，别让客人找不到。",
        ["job.book.r2"] = "你是经验丰富的旅行者吗？店里最近打听旧书和地图的人特别多，请帮我招呼柜台，顺便把散落的册子收齐。",
        ["job.book.r3"] = "新到的一批书还没拆完，柜台又排着人。我顾不上两头，请替我把书架理顺，问价的先记下来。",
        ["job.book.r4"] = "有人把专题书和闲书混在一起了。我正忙着进货，请找个细心点的人来重新排架，别把封面弄脏。",
        ["job.scholar.title"] = "学者助手",
        ["job.scholar.skill"] = "记忆",
        ["job.scholar.r1"] = "我缺了研究所需要的抄写人手。平时都是自己整理，但我现在脱不开身，请替我誊清几份记录，把案上的文书归好。",
        ["job.scholar.r2"] = "公会这边事务堆着，名册和回执都要过一遍。我不在的时候，请帮我把文件按类分开，别弄丢印章。",
        ["job.scholar.r3"] = "最近来办手续的人特别多，案头转不过来。请派个稳当的人来帮忙登记、传话，把排队的人也安顿好。",
        ["job.scholar.r4"] = "我手头有一份急件要核对。平时都是自己做，但今天实在顾不上，请替我把附表誊清并按编号收好。",

        // Board titles: shop identity, not generic clerk labels.
        ["job.shop.fish.need"] = "鱼店需要人手",
        ["job.shop.meat.need"] = "肉店需要人手",
        ["job.shop.fruit.need"] = "果蔬店需要人手",
        ["job.shop.bread.need"] = "面包店需要人手",
        ["job.shop.milk.need"] = "奶品店需要人手",
        ["job.shop.booze.need"] = "酒铺需要人手",
        ["job.shop.food.need"] = "食品店需要人手",
        ["job.shop.inn.need"] = "旅店需要人手",
        ["job.shop.kitchen.need"] = "厨房需要人手",
        ["job.shop.book.need"] = "书店需要人手",
        ["job.shop.scholar.need"] = "文书处需要人手",
        ["job.shop.smith.need"] = "铁匠铺需要人手",
        ["job.shop.general.need"] = "杂货店需要人手",
        ["job.shop.junk.need"] = "废品店需要人手",
        ["job.shop.souvenir.need"] = "纪念品店需要人手",
        ["job.shop.generic.need"] = "{0}需要人手",
    };

    static readonly Dictionary<string, string> En = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["term.dungeonExplore"] = "Dungeon Explore",
        ["term.regionDispatch"] = "Region Outing",
        ["term.dispatch"] = "Dispatch",
        ["term.townWork"] = "Shop Labor",
        ["term.coCraft"] = "Co-Craft",
        ["term.assist"] = "Assist",
        ["term.process"] = "Process",

        ["unlock.region"] = "{0} needs base Lv{1} (now {2})",
        ["unlock.dungeon"] = "{0} needs base Lv{1} (now {2})",

        ["reward.money"] = "Gold x{0}",
        ["reward.plat"] = "Platinum x{0}",
        ["reward.furnitureTicket"] = "Furniture ticket",
        ["reward.leftover"] = "Work bonus {0}",
        ["reward.skillDiscount"] = "A bit more practice would earn better pay...",
        ["reward.investTownCap"] = "Town investment level may need raising...",
        ["reward.investRaised"] = "{0}'s shop is more popular now...",

        ["town.settle.success"] = "done",
        ["town.settle.recall"] = "recalled",
        ["town.settle.fail"] = "failed",
        ["town.settle.line"] = "{0}'s \"{1}\" {2}.",
        ["town.you"] = "You",

        ["town.ui.self"] = "1  Do it yourself",
        ["town.ui.companion"] = "2  Send a companion",
        ["town.ui.cancel"] = "Cancel",
        ["town.ui.selfSub"] = "~{0}h · costs stamina",
        ["town.ui.companionSub"] = "~{0}h · pick 1",
        ["town.ui.skillWarn"] = "Needs a steadier hand for full pay...",
        ["town.ui.stop"] = "Stop (no main prize)",
        ["town.ui.recall"] = "Recall worker (no main prize)",
        ["town.ui.close"] = "Close",
        ["town.leave.confirm"] = "Shop labor is still running. Leaving now keeps the job ticking — leave anyway?",
        ["town.leave.confirmSelf"] = "You are doing shop labor yourself. Choose before leaving:",
        ["town.leave.abort"] = "1  Quit job (fail)",
        ["town.leave.swap"] = "2  Hand off to an NPC",
        ["town.leave.cancel"] = "Cancel",
        ["town.leave.header"] = "Self shop labor · leave",
        ["town.leave.noCandidates"] = "No free companion to take over",
        ["town.leave.swapOk"] = "{0} takes over \"{1}\".",
        ["town.leave.swapFail"] = "Hand-off failed",
        ["town.leave.abortOk"] = "You quit \"{0}\".",

        ["dispatch.settle.success"] = "success",
        ["dispatch.settle.fail"] = "failed",
        ["dispatch.settle.recall"] = "recalled",
        ["dispatch.settle.end"] = "ended",
        ["dispatch.settle.line"] = "{0}'s {1} ({2}) {3}.",
        ["dispatch.fail.hint.dungeon"] = "Success might have brought boss spoils and fame.",
        ["dispatch.fail.hint.region"] = "Success might have brought a fuller haul.",

        // ---- common ----
        ["common.cancel"] = "Cancel",

        // ---- co-craft ----
        ["co.mode.off"] = "Off",
        ["co.mode.auto"] = "Auto",
        ["co.mode.autoName"] = "Auto · {0}",
        ["co.mode.autoUnknown"] = "Auto(?)",
        ["co.mode.you"] = "(you)",
        ["co.tag.party"] = "P",
        ["co.tag.home"] = "H",
        ["co.prefix.hobby"] = "Hobby",
        ["co.prefix.work"] = "Work",
        ["co.menu.noAssist"] = "No assist",
        ["co.menu.noOne"] = "No one available",
        ["co.menu.skillShort"] = "sk",
        ["co.msg.started"] = "{0} is crafting with you.",
        ["co.msg.assistantFallback"] = "assistant",
        ["co.msg.gainedExp"] = "{0} got more skilled at {1}.",
        ["co.msg.ended"] = "{0} ended.",
        ["co.msg.autoSet"] = "Auto-assist enabled.",
        ["co.msg.off"] = "Assist disabled.",
        ["co.msg.pinned"] = "{0} will assist.",
        ["co.msg.pinFailed"] = "Chosen assistant unavailable; assist disabled.",

        // ---- process ----
        ["proc.mode.self"] = "Self",
        ["proc.mode.selfYou"] = "Yourself",
        ["proc.mode.auto"] = "Auto",
        ["proc.mode.autoNone"] = "Auto (none)",
        ["proc.menu.selfProcess"] = "Process yourself",
        ["proc.menu.pickBest"] = "Pick the best fit",
        ["proc.menu.noOne"] = "No one available",
        ["proc.menu.skillShort"] = "sk",
        ["proc.msg.selfMode"] = "You will process it yourself.",
        ["proc.msg.autoSet"] = "Auto: {0} now. Fill the grid to start.",
        ["proc.msg.autoNone"] = "Auto: no one suitable nearby right now.",
        ["proc.msg.pinned"] = "{0} will process it. Fill the grid to start.",
        ["proc.msg.pinFailed"] = "Chosen worker unavailable; switched back to yourself.",
        ["proc.msg.busy"] = "A processing job is already running.",
        ["proc.msg.materialIncomplete"] = "Ingredients are incomplete.",
        ["proc.msg.noWorker"] = "No worker available; process yourself or pick again.",
        ["proc.msg.notEnoughMaterial"] = "Not enough ingredients.",
        ["proc.msg.changed"] = "Ingredients changed when starting.",
        ["proc.msg.cannotTake"] = "Could not take the ingredients.",
        ["proc.msg.cannotOperate"] = "{0} doesn't know this machine.",
        ["proc.msg.start"] = "{0} starts processing on {1} ({2} uses).",
        ["proc.msg.residentFallback"] = "resident",
        ["proc.msg.machineFallback"] = "processor",
        ["proc.msg.doneSkilled"] = "{0} finished {1} and is more skilled now.",
        ["proc.msg.done"] = "{0} finished {1}.",
        ["proc.msg.interrupted"] = "{0}'s {1} was interrupted.",
        ["proc.msg.stopped"] = "{0} stopped {1}.",
        ["proc.msg.noFuel"] = "Out of fuel; {0} stopped.",
        ["proc.msg.noMaterial"] = "Materials ran out; {0} stopped.",
        ["proc.msg.noMachine"] = "The machine is gone; {0} stopped.",
        ["proc.msg.stuck"] = "The path is blocked; {0} stopped.",
        ["proc.msg.exhausted"] = "{0} is exhausted and stopped.",
        ["proc.msg.ended"] = "{0}'s {1} is done.",
        ["proc.msg.zoneContinue"] = "{0} stays on the old map and keeps working.",
        ["proc.msg.zoneResume"] = "{0} resumes processing ({1} left).",

        // ---- craft UI ----
        ["craft.effLine"] = "{0} / Eff {1}",
        ["craft.effLineSkill"] = "{0} {1} / Eff {2}",

        // ---- dispatch ----
        ["dis.error.invalidTarget"] = "Invalid target",
        ["dis.error.notUnlocked"] = "{0} not unlocked",
        ["dis.error.tooManyTarget"] = "At most {0} per target",
        ["dis.error.tooManyDungeon"] = "At most {0} per dungeon",
        ["dis.error.cannotSend"] = "{0} can't be sent",
        ["dis.error.noOne"] = "No one to send",
        ["dis.error.regionBusy"] = "A {0} is already in progress there",
        ["dis.error.dungeonBusy"] = "A {0} is already in progress there",
        ["dis.error.needBase"] = "A base is required",
        ["dis.error.regionTile"] = "No enterable tile of that region nearby",
        ["dis.error.dungeonNo"] = "Can't {0} this dungeon",
        ["dis.start.region"] = "{0} set out on {1}: {2} ({3} people · explore {4}w)",
        ["dis.start.dungeon"] = "{0} set out on {1}: {2} ({3} people · ~{4} days)",
        ["dis.error.startFail"] = "Failed to start: {0}",
        ["dis.loot.none"] = "No haul yet",
        ["dis.parcel.harvest"] = "Dispatch harvest",
        ["dis.parcel.partial"] = "Dispatch partial harvest",
        ["dis.parcel.recall"] = "Dispatch recalled",
        ["dis.parcel.loot"] = "Dispatch spoils",
        ["dis.parcel.package"] = "Dispatch parcel",
        ["dis.loot.item"] = "item",
        ["dis.loot.noMission"] = "No mission",
        ["dis.loot.noPlan"] = "No planned haul",
        ["dis.loot.moreKinds"] = "...{0} kinds in all",
        ["dis.loot.totalLine"] = "{0} ({1} items x{2})",
        ["dis.loot.etcItems"] = "{0} ({1} items in all)",
        ["dis.loot.planFail"] = "Plan read failed: {0}",
        ["dis.loot.special"] = "special",
        ["dis.debug.nonRegion"] = "Non-region mission (computed on success) · tracking: ",
        ["dis.debug.harvestDump"] = "Dispatch success plan haul x{0}: ",
        ["dis.debug.output"] = "Printed {0} mission plan hauls",
        ["dis.debug.noMission"] = "No dispatch missions right now",
        ["dis.debug.forceDone"] = "Force-completed {0} dispatch(es) (success haul mailed)",
        ["dis.debug.notFound"] = "Dispatch mission {0} not found",
        ["dis.debug.forceDoneOne"] = "Force-completed dispatch #{0} {1}",
        ["dis.exp.explore"] = "explore",
        ["dis.exp.lockpick"] = "lockpick",
        ["dis.exp.gather"] = "gather",
        ["dis.exp.dig"] = "digging",
        ["dis.exp.lumber"] = "lumberjack",
        ["dis.exp.tunnel"] = "tunneling",
        ["dis.exp.zero"] = "{0}+0",
        ["dis.exp.gain"] = "{0}+{1} (lv{2} to {3})",
        ["dis.compass.e"] = "East",
        ["dis.compass.w"] = "West",
        ["dis.compass.s"] = "South",
        ["dis.compass.n"] = "North",
        ["dis.compass.near"] = "nearby",
        ["dis.yield.rich"] = "rich",
        ["dis.yield.medium"] = "medium",
        ["dis.yield.normal"] = "normal",
        ["dis.yield.meager"] = "meager",
        ["dis.stat.none"] = "none selected",
        ["dis.stat.line"] = "Power {0}  Explore {1}  Lockpick {2}  Gather {3}",
        ["dis.stat.lineChance"] = "Power {0}  Explore {1}  Lockpick {2}  Gather {3}  {4}%",
        ["dis.region.name.plain"] = "Plains",
        ["dis.region.name.forest"] = "Forest",
        ["dis.region.name.beach"] = "Beach",
        ["dis.region.name.mountain"] = "Mountain",
        ["dis.region.name.area"] = "Area",
        ["dis.dist"] = "{0} {1}d",
        ["dis.distDanger"] = "{0} {1}d danger{2}",
        ["dis.kind.random"] = "random",
        ["dis.kind.fixed"] = "fixed",
        ["dis.kind.randomDungeon"] = "random dungeon",
        ["dis.kind.fixedDungeon"] = "fixed dungeon",
        ["dis.preview.region"] = "{0}\n{1}  {2}\n{3} people · {4}\nExplore {5} weeks\nExplore {6}  Lockpick {7}  Gather {8}",
        ["dis.preview.dungeon"] = "{0}\n{1}  Danger {2}  {3}\n{4} people · {5}\n~{6} days · Success {7}%\nPower {8}  Explore {9}  Lockpick {10}  Gather {11}",
        ["dis.info.dungeon"] = "{0} ({1}) {2} Danger {3} · ~{5}d",
        ["dis.info.region"] = "{0} (region) {1} · explore {2} weeks",
        ["dis.msg.baseOnly"] = "{0} can only be used at a base.",
        ["dis.msg.busyTalk"] = "{0} is exploring and can't talk.",
        ["dis.msg.busyInteract"] = "{0} is exploring and can't be interacted with.",
        ["dis.msg.empty"] = "No {0}/{1} right now",
        ["dis.ui.activeHeader"] = "— Active missions —",
        ["dis.ui.randomHeader"] = "— {0} · Random —",
        ["dis.ui.fixedHeader"] = "— {0} · Fixed —",
        ["dis.ui.noTargets"] = "No known targets nearby",
        ["dis.ui.recallPartial"] = "Recall (partial rewards)",
        ["dis.ui.recallFail"] = "Recall failed",
        ["dis.ui.progress"] = "{0}%  {1}d left",
        ["dis.ui.taskHeader"] = "Mission · {0}",
        ["dis.ui.weeks"] = "Explore {0} weeks",
        ["dis.ui.chooseWeeks"] = "{0} · Choose explore weeks",
        ["dis.ui.noLastTeam"] = "No previous team",
        ["dis.ui.noAutoPick"] = "No members to auto-pick",
        ["dis.ui.noCandidates"] = "No residents or allies to send",
        ["dis.ui.confirmSel"] = "Confirm ({0}/{1})",
        ["dis.ui.lastTeam"] = "Last team",
        ["dis.ui.maxHarvest"] = "Max haul",
        ["dis.ui.maxCombat"] = "Max combat haul",
        ["dis.ui.partyTag"] = "Party",
        ["dis.ui.restoreLast"] = "Restore last team",
        ["dis.ui.autoPickGather"] = "Auto-pick by explore/gather",
        ["dis.ui.autoPickPower"] = "Auto-pick by combat power",
        ["dis.ui.pickHeader"] = "{0} · Pick members (multi, max {1})",
        ["dis.ui.pickHeaderWeeks"] = "{0} · explore {1} weeks · pick (max {2})",
        ["dis.ui.tooMany"] = "At most {0} per dungeon",
        ["dis.ui.pickFirst"] = "Pick members first",
        ["dis.ui.invalidChoice"] = "Invalid choice",
        ["dis.ui.confirmSend"] = "Confirm dispatch",
        ["dis.ui.busySuffix"] = " (in progress)",
        ["dis.ui.inProgress"] = "In progress",
        ["dis.q.ended"] = " (ended)",
        ["dis.q.members"] = "Members: {0}",
        ["dis.q.position"] = "Position: {0}",
        ["dis.q.progress"] = "Progress {0}%",
        ["dis.q.left"] = "{0}d left",
        ["dis.q.harvest"] = "Haul: {0}",
        ["dis.q.regionPos"] = "{0} · {1} weeks",
        ["dis.q.target"] = "target",
        ["dis.q.entrance"] = "entrance",
        ["dis.q.area"] = "area",
        ["dis.q.plan"] = "Planned haul: {0}",

        // ---- town labor ----
        ["town.error.invalidJob"] = "Invalid job",
        ["town.error.noTownWork"] = "No {0} at this base",
        ["town.error.invalidPlace"] = "Invalid location",
        ["town.error.busy"] = "You're busy",
        ["town.error.busyCoCraft"] = "Currently co-crafting",
        ["town.error.busyProcess"] = "{0} in progress",
        ["town.error.clientBusy"] = "This client already has {0} in progress",
        ["town.error.maxConcurrent"] = "At most {0} {1} in town at once",
        ["town.error.jobGone"] = "The client's post is gone",
        ["town.error.noSp"] = "Too tired; rest before working",
        ["town.error.workerBusy"] = "Worker busy",
        ["town.error.cannotAssign"] = "{0} can't be assigned",
        ["town.msg.youStart"] = "You head to {0} for \"{1}\" (~{2}h).",
        ["town.msg.workerStart"] = "{0} heads to {1} for \"{2}\" (~{3}h).",
        ["town.msg.youTired"] = "Exhausted; stopped \"{0}\".",
        ["town.msg.workStart"] = "You start \"{1}\" at {0}.",
        ["town.msg.endedShort"] = "That {0} ended",
        ["town.msg.stopFail"] = "Couldn't stop",
        ["town.msg.recallFail"] = "Recall failed",
        ["town.msg.jobInvalid"] = "Invalid job",
        ["town.msg.clientGone"] = "Client is away",
        ["town.msg.noCandidates"] = "No free residents or allies",
        ["town.msg.invalidChoice"] = "Invalid choice",
        ["town.msg.town"] = "shop",
        ["town.msg.townName"] = "town",
        ["town.msg.worker"] = "worker",
        ["town.msg.companion"] = "companion",
        ["town.msg.client"] = "client",
        ["town.progress.arriving"] = "Heading to work...",
        ["town.progress.line"] = "{0}%  {1}h left",
        ["town.ui.confirmStart"] = "Confirm start",
        ["town.ui.sendConfirm"] = "Send {0} to \"{1}\"",
        ["town.ui.hours"] = "~{0}h",
        ["town.ui.choosePick"] = "{0} · pick 1 (~{1}h)",
        ["town.ui.skillFallback"] = "skill",
        ["town.ui.partyTag"] = "Party",
        ["town.ui.busyTag"] = "{0} in progress",
        ["town.ui.youTag"] = "· you",
        ["town.ui.needHelp"] = "The shop is busy and needs help.",
        ["town.ui.inProgress"] = "In progress",
        ["town.ui.header"] = "{0} · {1} · {2}",
        ["town.reward.hint"] = "Pay: about gold x{0}",
        ["town.reward.line"] = "Reward {0}",
        ["town.reward.moneyOnly"] = "Pay: gold x{0}",
        ["town.busy.invalid"] = "Invalid character",
        ["town.busy.town"] = "{0} in progress",
        ["town.busy.dungeon"] = "{0} in progress",
        ["town.busy.process"] = "{0} in progress",
        ["town.busy.coCraft"] = "Currently assisting craft",
        ["town.talk.busy"] = "{0} is busy with {1} and can't talk.",
        ["town.talk.noInteract"] = "{0} is busy with {1} and can't be interacted with.",

        // ---- job catalog ----
        ["job.inn.title"] = "Inn Chore",
        ["job.inn.skill"] = "Carrying/Chores",
        ["job.inn.r1"] = "The town is crowded lately; rooms and the hall are overrun. I'm busy with guests — please tidy the rooms and take care of the tables and linens downstairs.",
        ["job.inn.r2"] = "Adventurers are packing the inn, footsteps all night. I can't get away — send someone quick to make beds, pour water, and tidy the halls.",
        ["job.inn.r3"] = "Regulars arrive tomorrow and today is tight. I usually handle the chores myself but can't right now — please cover the shop's odd jobs for a while.",
        ["job.inn.r4"] = "Dishes and luggage are piled up and the front desk is queued. I'm swamped — have someone tidy the rooms and common areas until we can host again.",
        ["job.kitchen.title"] = "Kitchen Help",
        ["job.kitchen.skill"] = "Cooking",
        ["job.kitchen.r1"] = "The dinner rush is here and prep isn't done. I normally prep and wash myself but can't step away — lend a hand in the kitchen.",
        ["job.kitchen.r2"] = "The stove is blazing and dishes are piling up. I'm on the mains — send someone to wash pots, pass plates, and watch the pickup window.",
        ["job.kitchen.r3"] = "Orders are up and the kitchen can't keep pace. No cooking needed — just hold the line on chores so plates keep going out.",
        ["job.kitchen.r4"] = "Short-handed suddenly, with stocks and sides backed up. Have a careful person prep and wash; we just need to get through this stretch.",
        ["job.general.title"] = "General Clerk",
        ["job.general.skill"] = "Bartering",
        ["job.general.r1"] = "The shelves are messy and customers are queuing at the door. I need to check stock — watch the counter, straighten goods, and greet regulars.",
        ["job.general.r2"] = "New stock hasn't been shelved and no one can leave the floor. I usually do it myself but can't now — send someone to help.",
        ["job.general.r3"] = "Too many customers for one person lately. Find someone clear-spoken to mind the shop — if unsure of a price, call me.",
        ["job.general.r4"] = "Chores are piling up — ropes, candles, jars all need rearranging. I'm on the books; straighten the shelves and handle the counter.",
        ["job.smith.title"] = "Smith's Apprentice",
        ["job.smith.skill"] = "Smithing",
        ["job.smith.r1"] = "The forge is hot and orders are backed up. I can't step away — work the bellows, hand me tongs, and gather the filings.",
        ["job.smith.r2"] = "Forging is booked solid today. No hammering needed — pass tools, clear the bench, and move with the rhythm.",
        ["job.smith.r3"] = "Pieces and scrap cover the anvil bench. I usually clean up myself but I'm rushing orders — send an assistant.",
        ["job.smith.r4"] = "Short an extra hand; the bellows and scrap are slipping. Find someone steady — and warn them the metal is hot.",
        ["job.food.title"] = "Food Clerk",
        ["job.food.skill"] = "Cooking/Trading",
        ["job.food.r1"] = "The morning stock just came in and the scale is busy. I can't step away — arrange the goods and serve customers at the scale.",
        ["job.food.r2"] = "More buyers than usual; one person can't cover the counter. Send someone to set out goods and weigh, stowing the ugly ones behind.",
        ["job.food.r3"] = "Fresh goods sell fast and the baskets aren't sorted. I usually do it myself but can't now — have someone help.",
        ["job.food.r4"] = "The counter is busy and deliveries are backed up. Put the food out front, greet buyers warmly, and ride out the rush.",
        ["job.book.title"] = "Bookstore Clerk",
        ["job.book.skill"] = "Reading",
        ["job.book.r1"] = "I'm short hands for shelving. I normally sort and number myself but can't now — put the new books on their shelves so customers can find them.",
        ["job.book.r2"] = "Are you a seasoned traveler? Lately many ask after old books and maps — mind the counter and gather the stray volumes.",
        ["job.book.r3"] = "New books are unpacked halfway and the counter is queued. I can't cover both — straighten the shelves and jot down price queries.",
        ["job.book.r4"] = "Someone mixed the specialist books with the leisure ones. I'm busy restocking — have a careful person re-shelve without soiling the covers.",
        ["job.scholar.title"] = "Scholar's Aide",
        ["job.scholar.skill"] = "Memory",
        ["job.scholar.r1"] = "I need a copyist for my research. I usually organize it myself but can't now — transcribe a few records and file the documents.",
        ["job.scholar.r2"] = "Guild work is piling up — rosters and receipts need review. While I'm away, sort the files by type and don't lose the seal.",
        ["job.scholar.r3"] = "Processions are endless lately and the desk is overwhelmed. Send a steady person to register, relay messages, and manage the queue.",
        ["job.scholar.r4"] = "I have an urgent document to verify. I'd do it myself but can't today — copy the appendix cleanly and file it by number.",

        // Board titles: shop identity, not generic clerk labels.
        ["job.shop.fish.need"] = "Fish shop needs help",
        ["job.shop.meat.need"] = "Butcher needs help",
        ["job.shop.fruit.need"] = "Fruit shop needs help",
        ["job.shop.bread.need"] = "Bakery needs help",
        ["job.shop.milk.need"] = "Dairy shop needs help",
        ["job.shop.booze.need"] = "Liquor shop needs help",
        ["job.shop.food.need"] = "Food shop needs help",
        ["job.shop.inn.need"] = "Inn needs help",
        ["job.shop.kitchen.need"] = "Kitchen needs help",
        ["job.shop.book.need"] = "Bookstore needs help",
        ["job.shop.scholar.need"] = "Desk needs help",
        ["job.shop.smith.need"] = "Smithy needs help",
        ["job.shop.general.need"] = "General store needs help",
        ["job.shop.junk.need"] = "Junk shop needs help",
        ["job.shop.souvenir.need"] = "Souvenir shop needs help",
        ["job.shop.generic.need"] = "{0} needs help",
    };

    internal static string T(string key)
    {
        Dictionary<string, string> primary = PickTable();
        if (primary.TryGetValue(key, out string? s) && !string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (primary != En && En.TryGetValue(key, out s) && !string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (Cn.TryGetValue(key, out s) && !string.IsNullOrEmpty(s))
        {
            return s;
        }

        return key;
    }

    internal static string T(string key, params object[] args)
    {
        string fmt = T(key);
        if (args == null || args.Length == 0)
        {
            return fmt;
        }

        try
        {
            return string.Format(fmt, args);
        }
        catch
        {
            return fmt;
        }
    }

    /// <summary>
    /// Prefer English only when game language looks English.
    /// Uses reflection so missing Lang fields never break the build.
    /// </summary>
    static bool UseEnglish()
    {
        try
        {
            if (ProbeLangFlag("isEN"))
            {
                return true;
            }

            string? code = ProbeLangCode();
            if (code == null || code.Length == 0)
            {
                return false;
            }

            string c = code!.Trim().ToLowerInvariant();
            return c == "en" || c.StartsWith("en") || c.Contains("english");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Prefer Japanese when game language looks JP/JA.
    /// </summary>
    static bool UseJapanese()
    {
        try
        {
            if (ProbeLangFlag("isJP") || ProbeLangFlag("isJA"))
            {
                return true;
            }

            string? code = ProbeLangCode();
            if (code == null || code.Length == 0)
            {
                return false;
            }

            string c = code!.Trim().ToLowerInvariant();
            return c == "jp" || c == "ja" || c.StartsWith("jp") || c.StartsWith("ja")
                || c.Contains("japan");
        }
        catch
        {
            return false;
        }
    }

    static Dictionary<string, string> PickTable()
    {
        if (UseEnglish())
        {
            return En;
        }

        if (UseJapanese())
        {
            return Ja;
        }

        return Cn;
    }

    static bool ProbeLangFlag(string name)
    {
        try
        {
            Type? langType = typeof(EClass).Assembly.GetType("Lang")
                ?? Type.GetType("Lang");
            if (langType == null)
            {
                return false;
            }

            var f = langType.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (f != null && f.FieldType == typeof(bool))
            {
                return (bool)f.GetValue(null);
            }

            var prop = langType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                return (bool)prop.GetValue(null, null);
            }
        }
        catch
        {
        }

        return false;
    }

    static string? ProbeLangCode()
    {
        // Try common static members on Lang.
        try
        {
            Type? langType = typeof(EClass).Assembly.GetType("Lang")
                ?? Type.GetType("Lang");
            if (langType != null)
            {
                foreach (string name in new[] { "langCode", "lang", "id", "current", "Current" })
                {
                    try
                    {
                        var f = langType.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (f != null)
                        {
                            object? v = f.GetValue(null);
                            string? s = v as string ?? v?.ToString();
                            if (!string.IsNullOrEmpty(s))
                            {
                                return s;
                            }
                        }
                    }
                    catch
                    {
                    }

                    try
                    {
                        var prop = langType.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (prop != null)
                        {
                            object? v = prop.GetValue(null, null);
                            string? s = v as string ?? v?.ToString();
                            if (!string.IsNullOrEmpty(s))
                            {
                                return s;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch
        {
        }

        // Fallback: core config.lang if present.
        try
        {
            object? core = typeof(EClass).GetField("core", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null);
            if (core != null)
            {
                object? cfg = core.GetType().GetField("config")?.GetValue(core)
                    ?? core.GetType().GetProperty("config")?.GetValue(core, null);
                if (cfg != null)
                {
                    object? lang = cfg.GetType().GetField("lang")?.GetValue(cfg)
                        ?? cfg.GetType().GetProperty("lang")?.GetValue(cfg, null);
                    string? s = lang as string ?? lang?.ToString();
                    if (!string.IsNullOrEmpty(s))
                    {
                        return s;
                    }
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
