using SickDev.CommandSystem;
using SickDev.DevConsole;
using System.Collections.Generic;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

namespace _Project.Scripts.ConsoleCommands
{
    public class PerkCommands : BaseCommand
    {
#if UNITY_EDITOR
        private readonly string NAME = "Perks";
        [SerializeField]
        private List<PerkScriptableObject> perkList = new();

        protected override void Initialize()
        {
            DevConsole.singleton.AddCommand(command = new ActionCommand<string>(AddPerk) { className = NAME });
            commandsHolder.Add(command);

            DevConsole.singleton.AddCommand(command = new ActionCommand<string>(RemovePerk) { className = NAME });
            commandsHolder.Add(command);

            DevConsole.singleton.AddCommand(command = new ActionCommand(RemoveAllPerks) { className = NAME });
            commandsHolder.Add(command);

            DevConsole.singleton.AddCommand(command = new ActionCommand(AddAllPerks) { className = NAME });
            commandsHolder.Add(command);
            
            base.Initialize();
        }

        private void AddPerk(string perkName)
        {
            foreach(var perk in perkList)
            {
                if(perk.perkName == perkName)
                {
                    ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>()?.AddPerk(perk);
                    return;
                }
            }
        }

        private void RemovePerk(string perkName)
        {
            ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>()?.RemovePerkByName(perkName);
        }

        private void AddAllPerks()
        {
            foreach(var perk in perkList)
            {
                ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>()?.AddPerk(perk);
            }
        }

        private void RemoveAllPerks()
        {
            PerkScriptableObject[] perks = ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>()?.GetPerkList().ToArray();
            foreach (var perk in perks)
            {
                ReferenceManager.BlakeHeroCharacter?.GetComponent<PlayerPerkManager>()?.RemovePerk(perk);
            }
        }
#endif
    }
}
