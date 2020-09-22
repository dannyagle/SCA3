using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SCA3
{
    public class ScaCampaignBehavior : CampaignBehaviorBase
    {
        private readonly List<HeroLocation> _heroLocations;

        public ScaCampaignBehavior(List<HeroLocation> heroLocations)
        {
            _heroLocations = heroLocations;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnQuestStartedEvent.AddNonSerializedListener(this, OnQuestStarted);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnQuestStarted(QuestBase quest)
        {
            var isNewCampaign = Campaign.Current.CampaignGameLoadingType == Campaign.GameLoadingType.NewCampaign;
            var isRebuildClanQuest = quest.StringId.Equals("rebuild_player_clan_storymode_quest");
            if (!isNewCampaign || !isRebuildClanQuest)
            {
                return;
            }

            InformationManager.DisplayMessage(new InformationMessage($"SCA3: Quest is {quest.StringId}. Is a new campaign: {isNewCampaign}. SCA3 activated..."));
            AddScaHeroesToWorld(Campaign.Current.MobileParties);
        }

        private void AddScaHeroesToWorld(MBReadOnlyList<MobileParty> mobileParties)
        {
            if (mobileParties == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("SCA3: mobileParties was null! Abandoning SCA3 party setup...", Color.FromUint(4278255360U)));
                return;
            }

            var parties = mobileParties.ToList();

            InformationManager.DisplayMessage(
                new InformationMessage($"SCA3: Mobile parties has {parties.Count } parties in it. Filtering for SCA heroes...", Color.FromUint(4278255360U)));

            foreach (var scaHero in _heroLocations)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"SCA3: Looping through SCA hero list. We see { scaHero.HeroName}.", Color.FromUint(4278255360U)));

                try
                {
                    var party = parties
                        .Where(p => p.Leader?.Name != null && p.Leader.Name.ToString() == scaHero.HeroName)
                        .SingleOrDefault();

                    if (party == null)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage($"SCA3: party lead by SCA hero '{scaHero.HeroName} ({scaHero.HeroId})' not found in mobileParties! Skipping hero.", Color.FromUint(4278255360U)));
                        continue;
                    }

                    InformationManager.DisplayMessage(
                        new InformationMessage($"SCA3: Found party lead by SCA hero {scaHero.HeroName}! (It is {party.Id})", Color.FromUint(4278255360U)));

                    var settlement = Settlement.Find(scaHero.SettlementId);
                    ChangeOwnerOfSettlementAction.ApplyByDefault(party.LeaderHero, settlement);
                    party.Position2D = new Vec2(
                        settlement.Position2D.X + scaHero.XOffset, 
                        settlement.Position2D.Y + scaHero.YOffset);
                    party.SetMovePatrolAroundSettlement(settlement);
                }
                catch(Exception ex)
                {
                    InformationManager.DisplayMessage(
                        new InformationMessage($"SCA3: Error looping through hero locations on hero: {scaHero.HeroName}! Message: {ex.Message}"));
                }
            }
        }





    }
}
