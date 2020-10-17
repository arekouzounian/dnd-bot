using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dnd_bot
{
    public class WeaponHelper
    {
        private string path;
        private WeaponList weapons;

        public WeaponHelper()
        {
            weapons = new WeaponList()
            {
                Weapons = new List<Weapon>()
            };

            path = Path.Combine(Directory.GetCurrentDirectory(), "weapons.json").Replace(@"\", @"\\");
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(JsonConvert.SerializeObject(weapons));
                }
            }
        }

        public WeaponList GetWeapons()
        {
            using (StreamReader reader = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<WeaponList>(reader.ReadLine());
            }
        }

        public bool AddWeapon(string name, string damage, string damageType, string effects, ulong ownerID)
        {
            weapons = GetWeapons();
            var inCorrectFormat = checkForCorrectFormat(new string[] { name, damage, damageType, effects });
            if(!inCorrectFormat)
            {
                return false;
            }
            if(!weapons.Weapons.Contains(new Weapon(name, damage, damageType, effects, ownerID)))
            {
                weapons.Weapons.Add(new Weapon(name, damage, damageType, effects, ownerID));
            }
            SaveWeapons(weapons);
            return true;
        }

        private void SaveWeapons(WeaponList weaponList)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(JsonConvert.SerializeObject(weaponList));
            }
        }

        public bool RemoveWeapon(string weaponName, SocketUser weaponOwner)
        {
            var weaponsList = GetWeapons();
            foreach(var weapon in weaponsList.Weapons)
            {
                if(weapon.Name.ToLower() == weaponName.ToLower() && weapon.OwnerID == weaponOwner.Id)
                {
                    weaponsList.Weapons.Remove(weapon);
                    SaveWeapons(weaponsList);
                    return true;
                }
            }
            return false;
        }

        private bool checkForCorrectFormat(string[] itemsToCheck) //returns true for correct format, false for bad formatting
        {
            foreach(var item in itemsToCheck)
            {
                foreach(var character in item)
                {
                    if(character < 32 && character > 12)
                    {
                        return false;
                    }
                }
                if(item.Contains("\n") || item.Length > 500)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class Weapon
    {
        public string Name;
        public string DamageDice;
        public string DamageType;
        public string Effects;
        public ulong OwnerID;
        public Weapon(string name, string damageDice, string damageType, string effects, ulong ownerID)
        {
            Name = name;
            DamageDice = damageDice;
            DamageType = damageType;
            Effects = effects;
            OwnerID = ownerID; 
        }
    }
    public class WeaponList
    {
        public List<Weapon> Weapons;
    }
}