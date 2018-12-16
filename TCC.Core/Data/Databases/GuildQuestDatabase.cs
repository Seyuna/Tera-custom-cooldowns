﻿using System.Collections.Generic;
using System.IO;

namespace TCC.Data.Databases
{
    public class GuildQuestDatabase
    {
        public Dictionary<uint, GuildQuest> GuildQuests { get; }
        public GuildQuestDatabase(string lang)
        {
            GuildQuests = new Dictionary<uint, GuildQuest>();
            var f = File.OpenText(Path.Combine(App.DataPath, $"guild_quests/guild_quests-{lang}.tsv"));
            while (true)
            {
                var line = f.ReadLine();
                if (line == null) break;
                var s = line.Split('\t');
                var id = uint.Parse(s[0]);
                var str = s[1];
                //var zId = uint.Parse(s[2]);
                GuildQuests.Add(id, new GuildQuest(id, str));
            }
        }
    }
    public class GuildQuest
    {
        public uint Id { get;  }
        public string Title { get;  }
/*
        public uint ZoneId { get; }
*/

        public GuildQuest(uint id, string s)
        {
            Id = id;
            Title = s;
            //ZoneId = zId;
        }
    }

}
