using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SCA3
{
    public class Main : MBSubModuleBase
    {
        private string baseXDocPath = BasePath.Name + "Modules\\KnownWorld\\ModuleData\\";

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if(!(game.GameType is Campaign))
            {
                InformationManager.DisplayMessage(new InformationMessage("Game is not a campaign therefore SCA3 is not loading data."));
                return;
            }

            var heroLocations = GetConfiguredHeroLocations();
            if(heroLocations == null || !heroLocations.Any())
            {
                InformationManager.DisplayMessage(new InformationMessage("SCA3: no SCA hero information loaded."));
                return;
            }

            ((CampaignGameStarter)gameStarter).AddBehavior(new ScaCampaignBehavior(heroLocations));
            InformationManager.DisplayMessage(new InformationMessage("Added SCA3 behavior."));
        }


        private List<HeroLocation> GetConfiguredHeroLocations()
        {
            var locationFile = baseXDocPath + "locations.xml";
            var heroLocations = new List<HeroLocation>();

            if (!File.Exists(locationFile))
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"SCA3: Failed to load hero location file! No SCA3 heroes added to world."));
                return heroLocations;
            }

            try
            {
                var xdoc = XDocument.Load(locationFile);

                foreach (var hero in xdoc.Descendants("HeroLocation"))
                {
                    try
                    {
                        heroLocations.Add(new HeroLocation
                        {
                            HeroId = hero.Element("HeroId").Value,
                            HeroName = hero.Element("HeroName").Value,
                            SettlementId = hero.Element("SettlementId").Value,
                            Ownership = hero.Element("Ownership").Value,
                            XOffset = int.Parse(hero.Element("XOffset").Value),
                            YOffset = int.Parse(hero.Element("YOffset").Value)
                        });
                    }
                    catch
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage($"SCA3: Failed to load hero location for: {hero.Element("HeroName").Value}. Skipping hero..."));
                    }
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage($"SCA3: Failed to open hero location file which we know exists. Message: {ex.Message}"));
                return null;
            }

            InformationManager.DisplayMessage(
                new InformationMessage($"SCA3: Loaded { heroLocations.Count } SCA heroes!"));
            return heroLocations;
        }

    }
}
