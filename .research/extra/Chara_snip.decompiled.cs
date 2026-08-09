// ---- around 5823 ----
			int a = -10;
			if (IsPCFaction && !IsPCParty && (origin == null || !origin.IsPCParty))
			{
				a = -5;
			}
			ModAffinity(EClass.pc, a, show: false);
			string text4 = id;
			if (!(text4 == "quru"))
			{
				if (text4 == "corgon")
				{
					EClass.game.cards.globalCharas.Find("quru")?.ModAffinity(EClass.pc, -20, show: false);
				}
			}
			else
			{
				EClass.game.cards.globalCharas.Find("corgon")?.ModAffinity(EClass.pc, -20, show: false);
			}
		}
		if (origin != null)
		{
			if (origin.IsPCParty || origin.IsPCPartyMinion)
			{
				int num = 0;
				if (trait is TraitMerchantTravel)
				{
					num = -20;
				}
				else if (OriginalHostility >= Hostility.Friend && IsHuman && !base.IsPCFactionOrMinion)
				{
					num = -5;
				}
				else if (race.id == "cat" && OriginalHostility >= Hostility.Neutral)
				{
					EClass.pc.Say("killcat");
					num = -1;
// ---- around 5829 ----
			string text4 = id;
			if (!(text4 == "quru"))
			{
				if (text4 == "corgon")
				{
					EClass.game.cards.globalCharas.Find("quru")?.ModAffinity(EClass.pc, -20, show: false);
				}
			}
			else
			{
				EClass.game.cards.globalCharas.Find("corgon")?.ModAffinity(EClass.pc, -20, show: false);
			}
		}
		if (origin != null)
		{
			if (origin.IsPCParty || origin.IsPCPartyMinion)
			{
				int num = 0;
				if (trait is TraitMerchantTravel)
				{
					num = -20;
				}
				else if (OriginalHostility >= Hostility.Friend && IsHuman && !base.IsPCFactionOrMinion)
				{
					num = -5;
				}
				else if (race.id == "cat" && OriginalHostility >= Hostility.Neutral)
				{
					EClass.pc.Say("killcat");
					num = -1;
				}
				if (EClass.pc.party.HasElement(1563) && num < 0)
				{
					num = num * 30 / 100;
				}
				if (num != 0)
// ---- around 5834 ----
					EClass.game.cards.globalCharas.Find("quru")?.ModAffinity(EClass.pc, -20, show: false);
				}
			}
			else
			{
				EClass.game.cards.globalCharas.Find("corgon")?.ModAffinity(EClass.pc, -20, show: false);
			}
		}
		if (origin != null)
		{
			if (origin.IsPCParty || origin.IsPCPartyMinion)
			{
				int num = 0;
				if (trait is TraitMerchantTravel)
				{
					num = -20;
				}
				else if (OriginalHostility >= Hostility.Friend && IsHuman && !base.IsPCFactionOrMinion)
				{
					num = -5;
				}
				else if (race.id == "cat" && OriginalHostility >= Hostility.Neutral)
				{
					EClass.pc.Say("killcat");
					num = -1;
				}
				if (EClass.pc.party.HasElement(1563) && num < 0)
				{
					num = num * 30 / 100;
				}
				if (num != 0)
				{
					EClass.player.ModKarma(num);
				}
			}
			if (origin == EClass.pc)
// ---- around 5870 ----
			{
				EClass.pc.faith.Revelation("kill", 10);
			}
			else if (origin.IsPCFaction)
			{
				origin.Chara.ModAffinity(EClass.pc, 1, show: false);
				origin.Chara.ShowEmo(Emo.love);
			}
		}
		if (base.sourceBacker != null && origin != null && origin.IsPCParty)
		{
			EClass.player.doneBackers.Add(base.sourceBacker.id);
		}
		if (base.IsPCFactionOrMinion)
		{
			if (!IsPC && !IsMinion && EClass.game.config.autoCombat.abortOnAllyDead && EClass.player.TryAbortAutoCombat())
			{
				Msg.Say("abort_allyDead");
			}
		}
		else if (EClass.game.config.autoCombat.abortOnEnemyDead && EClass.player.TryAbortAutoCombat(immediate: false))
		{
			Msg.Say("abort_enemyDead");
		}
		if (IsPCParty && !IsPC)
		{
			EClass.pc.party.RemoveMember(this);
			base.c_wasInPcParty = true;
			EClass.pc.Say("allyDead");
		}
		switch (id)
		{
		case "littleOne":
			if (attackSource != AttackSource.Euthanasia && !IsPCFaction)
			{
				EClass.player.flags.little_killed = true;
// ---- around 6572 ----
		}
		if (headpat && this != c)
		{
			if (c.interest > 0)
			{
				c.ModAffinity(EClass.pc, 1 + EClass.rnd(3));
				c.interest -= 20 + EClass.rnd(10);
			}
			if (faith == EClass.game.religions.MoonShadow && c.IsPCParty)
			{
				foreach (Chara member in party.members)
				{
					if (!member.IsPC && CanSeeLos(member))
					{
						member.AddCondition<ConEuphoric>(100 + Evalue(6904) * 5);
					}
				}
			}
		}
		if (c.Evalue(1221) > 0)
		{
			int ele = ((c.MainElement == Element.Void) ? 924 : c.MainElement.id);
			if (c.id == "hedgehog_ether")
			{
				ele = 922;
			}
			Say("reflect_thorne", this, c);
			DamageHP(10L, ele, Power, AttackSource.Condition);
		}
	}

	public void Kiss(Chara c)
	{
		EClass.player.forceTalk = true;
		Talk("kiss", null, null, IsPC);
		PlaySound("kiss");
// ---- around 6614 ----
		}
		if (IsPC && this != c)
		{
			if (c.interest > 0)
			{
				c.ModAffinity(EClass.pc, num);
				c.interest -= 20 + EClass.rnd(10);
			}
			else
			{
				c.Say("affinityNone", c, EClass.pc);
			}
		}
		Effect.Get("love")._Play(pos, isSynced ? renderer.position : pos.Position(), 0f, c.pos);
	}

	public void Slap(Chara c, bool slapToDeath = false)
	{
		if (id == "olderyoungersister")
		{
			Steam.GetAchievement(ID_Achievement.OYS);
		}
		PlaySound("whip");
		Say("slap", this, c);
		c.PlayAnime(AnimeID.Shiver);
		c.DamageHP(slapToDeath ? (c.MaxHP * 2) : (5 + EClass.rndHalf(EClass.pc.MaxHP / 3)), 919, 100, AttackSource.Condition);
		c.OnInsulted();
		if (slapToDeath && c.IsAliveInCurrentZone)
		{
			c.Die();
		}
	}

	public Chara SetEnemy(Chara c = null)
	{
		enemy = c;
// ---- around 8367 ----
	public Room FindRoom()
	{
		return FindBed()?.owner.pos.cell.room;
	}

	public void ModAffinity(Chara c, int a, bool show = true, bool showOnlyEmo = false)
	{
		if (c == this)
		{
			return;
		}
		if (IsPC)
		{
			c.ModAffinity(EClass.pc, a, show);
		}
		else
		{
			if (!trait.CanChangeAffinity)
			{
				return;
			}
			int num = StatsHygiene.GetAffinityMod(EClass.pc.hygiene.GetPhase()) + (HasElement(417) ? 30 : 0) + (EClass.pc.HasCondition<ConSmoking>() ? (-30) : 0);
			if (IsPCFaction && homeBranch != null)
			{
				num += (int)Mathf.Sqrt(homeBranch.Evalue(2117)) * 5;
			}
			bool flag = a > 0;
			if (flag)
			{
				a = a * num / 100;
				if (affinity.GetLunchChance() > EClass.rnd(100) && GetInt(71) >= 0 && GetInt(71) < EClass.world.date.GetRaw())
				{
					SetInt(71, -1);
				}
			}
			if (show)
// ---- around 8375 ----
		{
			return;
		}
		if (IsPC)
		{
			c.ModAffinity(EClass.pc, a, show);
		}
		else
		{
			if (!trait.CanChangeAffinity)
			{
				return;
			}
			int num = StatsHygiene.GetAffinityMod(EClass.pc.hygiene.GetPhase()) + (HasElement(417) ? 30 : 0) + (EClass.pc.HasCondition<ConSmoking>() ? (-30) : 0);
			if (IsPCFaction && homeBranch != null)
			{
				num += (int)Mathf.Sqrt(homeBranch.Evalue(2117)) * 5;
			}
			bool flag = a > 0;
			if (flag)
			{
				a = a * num / 100;
				if (affinity.GetLunchChance() > EClass.rnd(100) && GetInt(71) >= 0 && GetInt(71) < EClass.world.date.GetRaw())
				{
					SetInt(71, -1);
				}
			}
			if (show)
			{
				if (a == 0)
				{
					if (!showOnlyEmo)
					{
						Say("affinityNone", this, c);
					}
				}
// ---- around 8795 ----
			t.ModNum(-1);
			c.Talk("ticket");
			switch (t.id)
			{
			case "ticket_massage":
				c.ModAffinity(EClass.pc, 10);
				EClass.pc.SetAI(new AI_Massage
				{
					target = c
				});
				break;
			case "ticket_armpillow":
				c.ModAffinity(EClass.pc, 20);
				EClass.pc.AddCondition<ConSleep>(300, force: true);
				c.SetAI(new AI_ArmPillow
				{
					target = EClass.pc
				});
				break;
			case "ticket_champagne":
				c.ModAffinity(EClass.pc, 10);
				c.AddCondition<ConChampagne>();
				break;
			}
			return;
		}
		if (t.id == "flyer")
		{
			stamina.Mod(-1);
			if (c.things.Find((Thing a) => a.id == "flyer") != null)
			{
				c.Talk("flyer_miss");
				DoHostileAction(c);
				return;
			}
			if (EClass.rnd(20) != 0 && c.CHA > EClass.rnd(base.CHA + Evalue(291) * 3 + 10))
// ---- around 8802 ----
				{
					target = c
				});
				break;
			case "ticket_armpillow":
				c.ModAffinity(EClass.pc, 20);
				EClass.pc.AddCondition<ConSleep>(300, force: true);
				c.SetAI(new AI_ArmPillow
				{
					target = EClass.pc
				});
				break;
			case "ticket_champagne":
				c.ModAffinity(EClass.pc, 10);
				c.AddCondition<ConChampagne>();
				break;
			}
			return;
		}
		if (t.id == "flyer")
		{
			stamina.Mod(-1);
			if (c.things.Find((Thing a) => a.id == "flyer") != null)
			{
				c.Talk("flyer_miss");
				DoHostileAction(c);
				return;
			}
			if (EClass.rnd(20) != 0 && c.CHA > EClass.rnd(base.CHA + Evalue(291) * 3 + 10))
			{
				Msg.Say("affinityNone", c, this);
				t.Destroy();
				elements.ModExp(291, 10f);
				return;
			}
			elements.ModExp(291, 50f);
// ---- around 8810 ----
				{
					target = EClass.pc
				});
				break;
			case "ticket_champagne":
				c.ModAffinity(EClass.pc, 10);
				c.AddCondition<ConChampagne>();
				break;
			}
			return;
		}
		if (t.id == "flyer")
		{
			stamina.Mod(-1);
			if (c.things.Find((Thing a) => a.id == "flyer") != null)
			{
				c.Talk("flyer_miss");
				DoHostileAction(c);
				return;
			}
			if (EClass.rnd(20) != 0 && c.CHA > EClass.rnd(base.CHA + Evalue(291) * 3 + 10))
			{
				Msg.Say("affinityNone", c, this);
				t.Destroy();
				elements.ModExp(291, 10f);
				return;
			}
			elements.ModExp(291, 50f);
		}
		if (t.id == "statue_weird")
		{
			EClass.pc.Say("statue_sell");
		}
		t.isGifted = true;
		c.nextUse = c.affinity.OnGift(t);
		if (!t.isDestroyed)
// ---- around 8863 ----
	{
		Say("give_erohon", this);
		AddCondition<ConParalyze>(50, force: true);
		AddCondition<ConConfuse>(50, force: true);
		AddCondition<ConFear>(1000, force: true);
		ModAffinity(EClass.pc, 100);
		t.Destroy();
		Talk("pervert");
	}

	public void GiveLovePotion(Chara c, Thing t)
	{
		c.Say("give_love", c, t);
		c.PlaySound(t.material.GetSoundDead());
		c.ShowEmo(Emo.angry);
		c.ModAffinity(EClass.pc, -20, show: false);
		c.Talk("pervert");
		t.Destroy();
	}

	public bool RequestProtection(Chara attacker, Action<Chara> action)
	{
		if (HasCondition<StanceTaunt>() || base.isRestrained || attacker == this)
		{
			return false;
		}
		if (host != null && host.isRestrained)
		{
			return false;
		}
		if (base.IsPCFactionOrMinion && attacker.IsPCFactionOrMinion)
		{
			return false;
		}
		bool flag = false;
		foreach (Chara chara in EClass._map.charas)
// ---- around 8873 ----
	public void GiveLovePotion(Chara c, Thing t)
	{
		c.Say("give_love", c, t);
		c.PlaySound(t.material.GetSoundDead());
		c.ShowEmo(Emo.angry);
		c.ModAffinity(EClass.pc, -20, show: false);
		c.Talk("pervert");
		t.Destroy();
	}

	public bool RequestProtection(Chara attacker, Action<Chara> action)
	{
		if (HasCondition<StanceTaunt>() || base.isRestrained || attacker == this)
		{
			return false;
		}
		if (host != null && host.isRestrained)
		{
			return false;
		}
		if (base.IsPCFactionOrMinion && attacker.IsPCFactionOrMinion)
		{
			return false;
		}
		bool flag = false;
		foreach (Chara chara in EClass._map.charas)
		{
			if (chara == attacker || chara.enemy == this || chara == this || chara.host != null || chara.IsDisabled || !chara.IsFriendOrAbove(this) || chara.conSuspend != null || (chara.IsPCParty && !IsPCParty) || (IsPCFaction && !chara.IsPCFaction) || (attacker.IsPCFactionOrMinion && chara.IsPCFactionOrMinion) || chara.HasElement(1277))
			{
				continue;
			}
			bool flag2 = chara.HasElement(1225) && !chara.GetBool(126);
			if ((!flag2 && (flag || EClass.rnd(2) == 0 || !chara.HasCondition<StanceTaunt>())) || chara.HasCooldown(1225))
			{
				continue;
			}
