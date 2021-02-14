using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dnd_bot
{
    class MonsterHelper
    {
        public string MonsterName;
        public MonsterHelper(string monsterName)
        {
            MonsterName = monsterName;
        }

        public async void printMonster(SocketCommandContext context)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var monster = MonsterName.Trim().ToLower().Replace(' ', '-');
            string webData;
            try
            {
                webData = wc.DownloadString("https://www.dnd5eapi.co/api/monsters/" + monster + '/');
                var jsonMonster = JsonConvert.DeserializeObject<MonsterRoot>(webData);
                var eb = new EmbedBuilder();
                eb.WithColor(Color.DarkPurple);
                eb.WithTitle("Monster Name: " + jsonMonster.name);
                eb.AddField("Type:", $"{jsonMonster.size} {jsonMonster.type}, {jsonMonster.alignment}");
                eb.AddField("Armor Class:", jsonMonster.armor_class);
                eb.AddField("Hit Points:", jsonMonster.hit_points);
                eb.AddField("Speed: ", getSpeed(jsonMonster));
                eb.AddField("Stats:", getStats(jsonMonster));
                getSavingThrowProficiencies(jsonMonster, eb);
                getSkills(jsonMonster, eb);
                getVulnerabilities(jsonMonster, eb);
                eb.AddField("Senses: ", getSenses(jsonMonster));
                getLanguages(jsonMonster, eb);
                eb.AddField("Challenge Rating (CR):", jsonMonster.challenge_rating);
                getSpecialSkills(jsonMonster, eb);
                eb.WithFooter("Reference: https://5thsrd.org/gamemaster_rules/monsters/" + monster.Replace("-", "_") + "/");
                await context.Channel.SendMessageAsync(null, false, eb.Build());
            }
            catch (System.Net.WebException)
            {
                await context.Channel.SendMessageAsync("That monster isn't in my bestiary."); //but this might help-> send 5 images
            }

        }

        public string getSpeed(MonsterRoot monster)
        {
            StringBuilder strB = new StringBuilder();
            strB.Append("Speed: ");

            if (monster.speed.walk != null)
            {
                strB.Append(monster.speed.walk + ", ");
            }
            if (monster.speed.fly != null)
            {
                strB.Append("fly " + monster.speed.fly + ", ");
            }
            if (monster.speed.swim != null)
            {
                strB.Append("swim " + monster.speed.swim);
            }

            return strB.ToString();
        }

        public string getStats(MonsterRoot monster)
        {
            return $"STR:{monster.strength}({getModifier(monster.strength)})\t" +
                $"DEX:{monster.dexterity}({getModifier(monster.dexterity)})\t" +
                $"CON:{monster.constitution}({getModifier(monster.constitution)})\t" +
                $"INT:{monster.intelligence}({getModifier(monster.intelligence)})\t" +
                $"WIS:{monster.wisdom}({getModifier(monster.wisdom)})\t" +
                $"CHA:{monster.charisma}({getModifier(monster.charisma)})";
        }

        public string getModifier(int skillNum)
        {
            int mod = (skillNum - 10) / 2;
            if (mod > 0)
            {
                return $"+{mod}";
            }
            return $"{mod}";
        }

        public EmbedBuilder getSavingThrowProficiencies(MonsterRoot monster, EmbedBuilder eb)
        {
            StringBuilder strB = new StringBuilder();
            bool hasProf = false;
            foreach (var proficiency in monster.proficiencies)
            {
                if (proficiency.name.Contains("Saving Throw"))
                {
                    strB.Append($"{proficiency.name.Remove(0, 14)} +{proficiency.value}, ");
                    hasProf = true;
                }
            }
            if (hasProf)
            {
                eb.AddField("Saving Throw Proficiencies: ", strB.ToString());
            }

            return eb;
        }

        public EmbedBuilder getSkills(MonsterRoot monster, EmbedBuilder eb)
        {
            StringBuilder strB = new StringBuilder();
            foreach (var skill in monster.proficiencies)
            {
                if (!skill.name.Contains("Saving Throw"))
                {
                    strB.Append($"{skill.name.Remove(0, 7)} +{skill.value}, ");
                }
            }
            if (strB.Length > 0)
            {
                eb.AddField("Skills:", strB.Append(" ").Replace(",  ", "").ToString());
            }

            return eb;
        }

        public EmbedBuilder getVulnerabilities(MonsterRoot monster, EmbedBuilder eb)
        {
            if (monster.damage_immunities.Count > 0)
            {
                StringBuilder strB = new StringBuilder();
                foreach (var immunity in monster.damage_immunities)
                {
                    strB.Append($"{immunity}, ");
                }
                eb.AddField("Damage Immunities: ", strB.ToString());
            }
            if (monster.damage_resistances.Count > 0)
            {
                StringBuilder strB = new StringBuilder();
                foreach (var resistance in monster.damage_resistances)
                {
                    strB.Append($"{resistance}, ");
                }
                eb.AddField("Damage Resistances: ", strB.ToString());
            }
            if (monster.damage_vulnerabilities.Count > 0)
            {
                StringBuilder strB = new StringBuilder();
                foreach (var vulnerability in monster.damage_vulnerabilities)
                {
                    strB.Append($"{vulnerability}, ");
                }
                eb.AddField("Damage Vulnerabilities: ", strB.ToString());
            }
            if (monster.condition_immunities.Count > 0)
            {
                StringBuilder strB = new StringBuilder();
                foreach (var immunity in monster.condition_immunities)
                {
                    strB.Append($"{immunity.name}, ");
                }
                eb.AddField("Condition Immunities: ", strB.ToString());
            }

            return eb;
        }

        public string getSenses(MonsterRoot monster)
        {
            StringBuilder strB = new StringBuilder();
            if (monster.senses.blindsight != null)
            {
                strB.Append($"Blindsight, ");
            }
            if (monster.senses.darkvision != null)
            {
                strB.Append($"Darkvision, ");
            }
            strB.Append($"Passive Perception {monster.senses.passive_perception}");

            return strB.ToString();
        }

        public EmbedBuilder getSpecialSkills(MonsterRoot monster, EmbedBuilder eb)
        {
            if (monster.special_abilities.Count > 0)
            {
                foreach (var ability in monster.special_abilities)
                {
                    if (ability.usage != null)
                    {
                        eb.AddField(ability.name, $"{ability.desc} ({ability.usage.type}/{ability.usage.times})");
                    }
                    else
                    {
                        eb.AddField(ability.name, ability.desc);
                    }
                }
            }
            return eb;
        }

        public EmbedBuilder getLanguages(MonsterRoot monster, EmbedBuilder eb)
        {
            if (monster.languages != "")
            {
                eb.AddField("Languages:", monster.languages);
            }
            return eb;
        }
    }


    public class Speed
    {
        public string walk { get; set; }
        public string fly { get; set; }
        public string swim { get; set; }
    }

    public class Proficiency
    {
        public string name { get; set; }
        public string url { get; set; }
        public int value { get; set; }
    }

    public class Senses
    {
        public string blindsight { get; set; }
        public string darkvision { get; set; }
        public int passive_perception { get; set; }
    }

    public class Usage
    {
        public string type { get; set; }
        public int times { get; set; }
    }

    public class SpecialAbility
    {
        public string name { get; set; }
        public string desc { get; set; }
        public Usage usage { get; set; }
    }

    public class Options
    {
        public int choose { get; set; }
        public List<List<MonsterDamageType>> from { get; set; }
    }

    public class MonsterDamageType
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class conditionImmunity
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class MonsterDamage
    {
        public MonsterDamageType damage_type { get; set; }
        public string damage_dice { get; set; }
    }

    public class DcType
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Dc
    {
        public DcType dc_type { get; set; }
        public int dc_value { get; set; }
        public string success_type { get; set; }
    }

    public class Usage2
    {
        public string type { get; set; }
        public string dice { get; set; }
        public int min_value { get; set; }
    }

    public class Action
    {
        public string name { get; set; }
        public string desc { get; set; }
        public Options options { get; set; }
        public int? attack_bonus { get; set; }
        public List<MonsterDamage> damage { get; set; }
        public Dc dc { get; set; }
        public Usage2 usage { get; set; }
    }

    public class DcType2
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Dc2
    {
        public DcType2 dc_type { get; set; }
        public int dc_value { get; set; }
        public string success_type { get; set; }
    }

    public class DamageType2
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Damage2
    {
        public DamageType2 damage_type { get; set; }
        public string damage_dice { get; set; }
    }

    public class LegendaryAction
    {
        public string name { get; set; }
        public string desc { get; set; }
        public Dc2 dc { get; set; }
        public List<Damage2> damage { get; set; }
    }

    public class MonsterRoot
    {
        public string _id { get; set; }
        public string index { get; set; }
        public string name { get; set; }
        public string size { get; set; }
        public string type { get; set; }
        public object subtype { get; set; }
        public string alignment { get; set; }
        public int armor_class { get; set; }
        public int hit_points { get; set; }
        public string hit_dice { get; set; }
        public Speed speed { get; set; }
        public int strength { get; set; }
        public int dexterity { get; set; }
        public int constitution { get; set; }
        public int intelligence { get; set; }
        public int wisdom { get; set; }
        public int charisma { get; set; }
        public List<Proficiency> proficiencies { get; set; }
        public List<object> damage_vulnerabilities { get; set; }
        public List<object> damage_resistances { get; set; }
        public List<string> damage_immunities { get; set; }
        public List<conditionImmunity> condition_immunities { get; set; }
        public Senses senses { get; set; }
        public string languages { get; set; }
        public double challenge_rating { get; set; }
        public List<SpecialAbility> special_abilities { get; set; }
        public List<Action> actions { get; set; }
        public List<LegendaryAction> legendary_actions { get; set; }
        public string url { get; set; }
    }
}
