using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dnd_bot
{
    class SpellHelper
    {
        string SpellName;
        public SpellHelper(string spellName)
        {
            SpellName = spellName;
        }

        public async void printSpell(SocketCommandContext Context)
        {
            var temp = SpellName.ToLower().Trim().Replace(' ', '-').Replace('/', '-');
            System.Net.WebClient wc = new System.Net.WebClient();
            string webData;
            try
            {
                webData = wc.DownloadString("https://www.dnd5eapi.co/api/spells/" + temp + '/');
                var deserializedData = JsonConvert.DeserializeObject<Root>(webData);
                //printing
                var eb = new EmbedBuilder();
                eb.Color = Color.Blue;
                eb.WithTitle($"Spell Name: {deserializedData.name}");
                eb.AddField("Level:", $"{fixNumberFormat(deserializedData)}-level {deserializedData.school.name.ToLower()}");
                eb.AddField("Classes: ", getClasses(deserializedData));
                eb.AddField("Casting Time:", deserializedData.casting_time);
                eb.AddField("Range:", deserializedData.range);
                eb.AddField("Components:", getComponents(deserializedData));
                eb.AddField("Duration:", isConcentration(deserializedData) + deserializedData.duration);
                eb.AddField("Description:", getDesc(deserializedData));
                atHigherLevels(deserializedData, eb);
                eb.WithFooter($"Reference: https://5thsrd.org/spellcasting/spells/" + temp.Replace("-", "_") + "/");
                await Context.Channel.SendMessageAsync(null, false, eb.Build());
            }
            catch(System.Net.WebException)
            {
                await Context.Channel.SendMessageAsync("That spell isn't in my spellbook.");
            }

        }

        public string fixNumberFormat(Root spell)
        {
            if (spell.level == 1)
            {
                return $"{spell.level}st";
            }
            else if (spell.level == 2)
            {
                return $"{spell.level}nd";
            }
            else if (spell.level == 3)
            {
                return $"{spell.level}rd";
            }
            else
            {
                return spell.level + "th";
            }
        }

        public string getComponents(Root spell)
        {
            StringBuilder strB = new StringBuilder();
            foreach(var letter in spell.components)
            {
                strB.Append(letter + ",");
            }
            if (spell.material != null)
            {
                strB.Append($" ({spell.material})");
            }
            return strB.ToString();
        }

        public string isConcentration(Root spell)
        {
            return spell.concentration ? "Concentration, " : "";
        }

        public EmbedBuilder atHigherLevels(Root spell, EmbedBuilder eb)
        {
            if(spell.higher_level != null)
            {
                return eb.AddField("At Higher Levels:", spell.higher_level[0]);
                
            }
            return eb;
        }

        public string getDesc(Root spell)
        {
            StringBuilder strB = new StringBuilder();
            foreach(var description in spell.desc)
            {
                if(strB.ToString().Length + description.Length < 1024)
                {
                    strB.Append(description);
                    strB.Append("\n");
                }
            }
            return strB.ToString();
        }

        public string getClasses(Root spell)
        {
            StringBuilder strB = new StringBuilder();
            foreach(var pcClass in spell.classes)
            {
                strB.Append(pcClass.name + ", ");
            }
            return strB.Append(" ").Replace(",  ", "").ToString();
        }
    }

    public class DamageType
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class DamageAtSlotLevel
    {
        public string two { get; set; }
        public string three { get; set; }
        public string four { get; set; }
        public string five { get; set; }
        public string six { get; set; }
        public string seven { get; set; }
        public string eight { get; set; }
        public string nine { get; set; }
    }

    public class Damage
    {
        public DamageType damage_type { get; set; }
        public DamageAtSlotLevel damage_at_slot_level { get; set; }
    }

    public class School
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Class
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Subclass
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Root
    {
        public string _id { get; set; }
        public string index { get; set; }
        public string name { get; set; }
        public List<string> desc { get; set; }
        public List<string> higher_level { get; set; }
        public string range { get; set; }
        public List<string> components { get; set; }
        public string material { get; set; }
        public bool ritual { get; set; }
        public string duration { get; set; }
        public bool concentration { get; set; }
        public string casting_time { get; set; }
        public int level { get; set; }
        public string attack_type { get; set; }
        public Damage damage { get; set; }
        public School school { get; set; }
        public List<Class> classes { get; set; }
        public List<Subclass> subclasses { get; set; }
        public string url { get; set; }
    }
}
