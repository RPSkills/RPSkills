using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Vintagestory.API.Datastructures;

namespace rpskills.OriginSys
{
    public class Origin
    {
        public string Name;

        /// <summary>
        /// key: skill; value: level
        /// </summary>
        public Dictionary<string, int> Skillset;
    }

    public class Skill
    {
        public string Name;

        public int Level;

        /// <summary>
        /// key: attribute; value: modifier
        /// </summary>
        public List<string> Paths;

        public override string ToString()
        {
            string result = "Name: " + Name;
            result += "\nLevel: " + Level;
            result += "\nPaths:";
            foreach (var path in Paths)
            {
                result += "\n - " + path;
            }

            return result;
        }
    }

    public class Path
    {
        public string Name;
        public string Value;

        public override string ToString()
        {
            return "[Name: " + Name + "] Value: " + Value;
        }
    }

    /*{
    'Command': '',
    'Syntax': '',
    'Description': '',
    'Required Privl': '',
    'handler': 'somefuncdefinedinthisfile'
    }*/

    public class Command
    {
        public string Name;
        public string Description;
        public string Privilege;
        public string Handler;
    }
}