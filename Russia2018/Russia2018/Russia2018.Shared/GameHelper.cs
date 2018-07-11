using Newtonsoft.Json;
using Russia2018.Model;
using Russia2018.Shared.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;

namespace Russia2018
{
    public class GameHelper
    {
        static GameHelper instance;

        private GameHelper()
        {
        }

        public async Task Initialize()
        {
            Groups = new List<Group>();
            TeamCodes = new List<string>();
            TeamsDictionary = new Dictionary<string, SoccerTeam>();
            AddGroups();
            await AddTeams();
            AddTeamCodes();
        }

        private void AddTeamCodes()
        {
            TeamCodes.Add("RUS");// Russia                43965 
            TeamCodes.Add("KSA");// Saudi Arabia          43835 
            TeamCodes.Add("EGY");// Egypt                 43855 
            TeamCodes.Add("URU");// Uruguay               43930 
            TeamCodes.Add("POR");// Portugal              43963 
            TeamCodes.Add("ESP");// Spain                 43969 
            TeamCodes.Add("MAR");// Morocco               43872 
            TeamCodes.Add("IRN");// IR Iran               43817 
            TeamCodes.Add("FRA");// France                43946 
            TeamCodes.Add("AUS");// Australia             43976 
            TeamCodes.Add("PER");// Peru                  43929 
            TeamCodes.Add("DEN");// Denmark               43941 
            TeamCodes.Add("ARG");// Argentina             43922 
            TeamCodes.Add("ISL");// Iceland               43951 
            TeamCodes.Add("CRO");// Croatia               43938 
            TeamCodes.Add("NGA");// Nigeria               43876 
            TeamCodes.Add("BRA");// Brazil                43924 
            TeamCodes.Add("SUI");// Switzerland           43971 
            TeamCodes.Add("CRC");// Costa Rica            43901 
            TeamCodes.Add("SRB");// Serbia                1902465  
            TeamCodes.Add("GER");// Germany               43948 
            TeamCodes.Add("MEX");// Mexico                43911 
            TeamCodes.Add("SWE");// Sweden                43970 
            TeamCodes.Add("KOR");// Korea Republic        43822 
            TeamCodes.Add("BEL");// Belgium               43935 
            TeamCodes.Add("PAN");// Panama                43914 
            TeamCodes.Add("TUN");// Tunisia               43888 
            TeamCodes.Add("ENG");// England               43942 
            TeamCodes.Add("POL");// Poland                43962 
            TeamCodes.Add("SEN");// Senegal               43879 
            TeamCodes.Add("COL");// Colombia              43926 
            TeamCodes.Add("JPN");// Japan                 43819 
        }

        public static GameHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameHelper();
                    instance.Initialize().Wait();
                }

                return instance;
            }
        }

        public Player CurrentMousePlayer { get; set; }
        public Player CurrentSelectedPlayer { get; set;}
        public bool IsMovingDiscoids { get; set; }
        public List<Group> Groups { get; set; }
        public Dictionary<string, SoccerTeam> TeamsDictionary { get; set; }
        public List<string> TeamCodes { get; set; }
        public WorldCupData WorldCupData { get; set; }

        void AddGroups()
        {
            Groups.Add(new Group() { GroupID ="A" });
            Groups.Add(new Group() { GroupID ="B" });
            Groups.Add(new Group() { GroupID ="C" });
            Groups.Add(new Group() { GroupID ="D" });
            Groups.Add(new Group() { GroupID ="E" });
            Groups.Add(new Group() { GroupID ="F" });
            Groups.Add(new Group() { GroupID ="G" });
            Groups.Add(new Group() { GroupID ="H" });
        }

        async Task AddTeams()
        {
            WorldCupData = await LoadJsonData();

            AddGroupTeams(WorldCupData, "A", WorldCupData.groups.a.matches);
            AddGroupTeams(WorldCupData, "B", WorldCupData.groups.b.matches);
            AddGroupTeams(WorldCupData, "C", WorldCupData.groups.c.matches);
            AddGroupTeams(WorldCupData, "D", WorldCupData.groups.d.matches);
            AddGroupTeams(WorldCupData, "E", WorldCupData.groups.e.matches);
            AddGroupTeams(WorldCupData, "F", WorldCupData.groups.f.matches);
            AddGroupTeams(WorldCupData, "G", WorldCupData.groups.g.matches);
            AddGroupTeams(WorldCupData, "H", WorldCupData.groups.h.matches);
        }

        private void AddGroupTeams(WorldCupData jsonData, string groupName, List<Match> matches)
        {
            var teamIdsInGroup =
                matches.Select(m => m.home_team)
                .Union(matches.Select(m => m.away_team))
                .Distinct();
            var teamsInGroup =
                from t in jsonData.teams
                from id in teamIdsInGroup
                where t.id == id
                select t;

            foreach (var team in teamsInGroup)
            {
                TeamsDictionary.Add(team.fifaCode, new SoccerTeam() { TeamID = team.fifaCode, TeamName = team.name, NumberColor = Colors.White, GroupID = groupName });
            }
        }

        async Task<WorldCupData> LoadJsonData()
        {
            Uri appUri = new Uri("ms-appx:///Assets/data.json");
            StorageFile anjFile = StorageFile.GetFileFromApplicationUriAsync(appUri).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            string jsonText = FileIO.ReadTextAsync(anjFile).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
            var rootObject = JsonConvert.DeserializeObject<WorldCupData>(jsonText);
            return await Task.FromResult(rootObject);
        }
    }
}
