using System;
using System.Collections.Generic;

namespace rpskills.CoreSys
{
    public class Heritage
    {
        public string Name;

        /// <summary>
        /// key: skill; value: level
        /// </summary>
        public Dictionary<string, int> Skillset;
    }

    // TODO(chris): move to its own file
    public class Skill
    {
        public string Name;

        public int Level;

        /// <summary>
        /// key: attribute; value: modifier
        /// </summary>
        public List<string> Paths;
    }

    public class Path
    {
        public string Name;
        public string Value;
    }
}