using Russia2018.Model;
using System.Collections.Generic;
using Windows.UI;

namespace Russia2018
{
    public class GameHelper
    {
        static GameHelper instance;

        private GameHelper()
        {
            Groups = new List<Group>();
            TeamCodes = new List<string>();
            TeamsDictionary = new Dictionary<string, Team>();
            AddGroups();
            AddTeams();
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
                    instance = new GameHelper();
                return instance;
            }
        }

        public Player CurrentMousePlayer { get; set; }
        public Player CurrentSelectedPlayer { get; set;}
        public bool IsMovingDiscoids { get; set; }
        public List<Group> Groups { get; set; }
        public Dictionary<string, Team> TeamsDictionary { get; set; }
        public List<string> TeamCodes { get; set; }

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

        void AddTeams()
        {
            TeamsDictionary.Add("RUS", new Team() { TeamID ="RUS", TeamName ="Russia", NumberColor = Colors.White, GroupID ="A" });//43965 
            TeamsDictionary.Add("KSA", new Team() { TeamID ="KSA", TeamName ="Saudi Arabia", NumberColor = Colors.White, GroupID ="A" });//43835 
            TeamsDictionary.Add("EGY", new Team() { TeamID ="EGY", TeamName ="Egypt", NumberColor = Colors.White, GroupID ="A" });//43855 
            TeamsDictionary.Add("URU", new Team() { TeamID ="URU", TeamName ="Uruguay", NumberColor = Colors.White, GroupID ="A" });//43930 

            TeamsDictionary.Add("POR", new Team() { TeamID ="POR", TeamName ="Portugal", NumberColor = Colors.White, GroupID ="B" });//43963 
            TeamsDictionary.Add("ESP", new Team() { TeamID ="ESP", TeamName ="Spain", NumberColor = Colors.White, GroupID ="B" });//43969 
            TeamsDictionary.Add("MAR", new Team() { TeamID ="MAR", TeamName ="Morocco", NumberColor = Colors.White, GroupID ="B" });//43872 
            TeamsDictionary.Add("IRN", new Team() { TeamID ="IRN", TeamName ="IR Iran", NumberColor = Colors.White, GroupID ="B" });//43817 

            TeamsDictionary.Add("FRA", new Team() { TeamID ="FRA", TeamName ="France", NumberColor = Colors.White, GroupID ="C" });//43946 
            TeamsDictionary.Add("AUS", new Team() { TeamID ="AUS", TeamName ="Australia", NumberColor = Colors.White, GroupID ="C" });//43976 
            TeamsDictionary.Add("PER", new Team() { TeamID ="PER", TeamName ="Peru", NumberColor = Colors.White, GroupID ="C" });//43929 
            TeamsDictionary.Add("DEN", new Team() { TeamID ="DEN", TeamName ="Denmark", NumberColor = Colors.White, GroupID ="C" });//43941 

            TeamsDictionary.Add("ARG", new Team() { TeamID ="ARG", TeamName ="Argentina", NumberColor = Colors.White, GroupID ="D" });//43922 
            TeamsDictionary.Add("ISL", new Team() { TeamID ="ISL", TeamName ="Iceland", NumberColor = Colors.White, GroupID ="D" });//43951 
            TeamsDictionary.Add("CRO", new Team() { TeamID ="CRO", TeamName ="Croatia", NumberColor = Colors.White, GroupID ="D" });//43938 
            TeamsDictionary.Add("NGA", new Team() { TeamID ="NGA", TeamName ="Nigeria", NumberColor = Colors.White, GroupID ="D" });//43876 

            TeamsDictionary.Add("BRA", new Team() { TeamID ="BRA", TeamName ="Brazil", NumberColor = Colors.White, GroupID ="E" });//43924 
            TeamsDictionary.Add("SUI", new Team() { TeamID ="SUI", TeamName ="Switzerland", NumberColor = Colors.White, GroupID ="E" });//43971 
            TeamsDictionary.Add("CRC", new Team() { TeamID ="CRC", TeamName ="Costa Rica", NumberColor = Colors.White, GroupID ="E" });//43901 
            TeamsDictionary.Add("SRB", new Team() { TeamID ="SRB", TeamName ="Serbia", NumberColor = Colors.White, GroupID ="E" });//1902465  

            TeamsDictionary.Add("GER", new Team() { TeamID ="GER", TeamName ="Germany", NumberColor = Colors.White, GroupID ="F" });//43948 
            TeamsDictionary.Add("MEX", new Team() { TeamID ="MEX", TeamName ="Mexico", NumberColor = Colors.White, GroupID ="F" });//43911 
            TeamsDictionary.Add("SWE", new Team() { TeamID ="SWE", TeamName ="Sweden", NumberColor = Colors.White, GroupID ="F" });//43970 
            TeamsDictionary.Add("KOR", new Team() { TeamID ="KOR", TeamName ="Korea Republic", NumberColor = Colors.White, GroupID ="F" });//43822 

            TeamsDictionary.Add("BEL", new Team() { TeamID ="BEL", TeamName ="Belgium", NumberColor = Colors.White, GroupID ="G" });//43935 
            TeamsDictionary.Add("PAN", new Team() { TeamID ="PAN", TeamName ="Panama", NumberColor = Colors.White, GroupID ="G" });//43914 
            TeamsDictionary.Add("TUN", new Team() { TeamID ="TUN", TeamName ="Tunisia", NumberColor = Colors.White, GroupID ="G" });//43888 
            TeamsDictionary.Add("ENG", new Team() { TeamID ="ENG", TeamName ="England", NumberColor = Colors.White, GroupID ="G" });//43942 

            TeamsDictionary.Add("POL", new Team() { TeamID ="POL", TeamName ="Poland", NumberColor = Colors.White, GroupID ="H" });//43962 
            TeamsDictionary.Add("SEN", new Team() { TeamID ="SEN", TeamName ="Senegal", NumberColor = Colors.White, GroupID ="H" });//43879 
            TeamsDictionary.Add("COL", new Team() { TeamID ="COL", TeamName ="Colombia", NumberColor = Colors.White, GroupID ="H" });//43926 
            TeamsDictionary.Add("JPN", new Team() { TeamID ="JPN", TeamName ="Japan", NumberColor = Colors.White, GroupID ="H" });//43819 

            //TeamsDictionary.Add("RSA", new Team() { TeamID ="RSA", TeamName ="South Africa", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("MEX", new Team() { TeamID ="MEX", TeamName ="Mexico", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("URU", new Team() { TeamID ="URU", TeamName ="Uruguay", NumberColor = Colors.Black, GroupID ="B", R1 = 0x00, G1 = 0xE0, B1 = 0xE0, R2 = 0x00, G2 = 0xF0, B2 = 0xFF, R3 = 0x00, G3 = 0xF0, B3 = 0xFF });
            //TeamsDictionary.Add("FRA", new Team() { TeamID ="FRA", TeamName ="France", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("ARG", new Team() { TeamID ="ARG", TeamName ="Argentina", NumberColor = Colors.Black, GroupID ="B", R1 = 0x00, G1 = 0xE0, B1 = 0xE0, R2 = 0x00, G2 = 0xF0, B2 = 0xFF, R3 = 0x00, G3 = 0xF0, B3 = 0xFF });
            //TeamsDictionary.Add("NGA", new Team() { TeamID ="NGA", TeamName ="Nigeria", NumberColor = Colors.White, GroupID ="B", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("KOR", new Team() { TeamID ="KOR", TeamName ="South Korea", NumberColor = Colors.White, GroupID ="B", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("GRE", new Team() { TeamID ="GRE", TeamName ="Greece", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("ENG", new Team() { TeamID ="ENG", TeamName ="England", NumberColor = Colors.White, GroupID ="C", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("USA", new Team() { TeamID ="USA", TeamName ="United States", NumberColor = Colors.Black, GroupID ="C", R1 = 0xB0, G1 = 0xB0, B1 = 0xB0, R2 = 0xC0, G2 = 0xC0, B2 = 0xC0, R3 = 0xF0, G3 = 0xF0, B3 = 0xF0 });
            //TeamsDictionary.Add("ALG", new Team() { TeamID ="ALG", TeamName ="Algeria", NumberColor = Colors.White, GroupID ="C", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("SVN", new Team() { TeamID ="SVN", TeamName ="Slovenia", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("GER", new Team() { TeamID ="GER", TeamName ="Germany", NumberColor = Colors.Black, GroupID ="D", R1 = 0xB0, G1 = 0xB0, B1 = 0xB0, R2 = 0xC0, G2 = 0xC0, B2 = 0xC0, R3 = 0xF0, G3 = 0xF0, B3 = 0xF0 });
            //TeamsDictionary.Add("AUS", new Team() { TeamID ="AUS", TeamName ="Australia", NumberColor = Colors.Blue, GroupID ="G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            //TeamsDictionary.Add("SRB", new Team() { TeamID ="SRB", TeamName ="Serbia", NumberColor = Colors.White, GroupID ="D", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("GHA", new Team() { TeamID ="GHA", TeamName ="Ghana", NumberColor = Colors.White, GroupID ="D", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("NED", new Team() { TeamID ="NED", TeamName ="Netherlands", NumberColor = Colors.White, GroupID ="E", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("DEN", new Team() { TeamID ="DEN", TeamName ="Denmark", NumberColor = Colors.White, GroupID ="E", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("JPN", new Team() { TeamID ="JPN", TeamName ="Japan", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("CMR", new Team() { TeamID ="CMR", TeamName ="Cameroon", NumberColor = Colors.White, GroupID ="E", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("ITA", new Team() { TeamID ="ITA", TeamName ="Italy", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("PAR", new Team() { TeamID ="PAR", TeamName ="Paraguay", NumberColor = Colors.White, GroupID ="F", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("NZL", new Team() { TeamID ="NZL", TeamName ="New Zealand", NumberColor = Colors.White, GroupID ="F", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("SVK", new Team() { TeamID ="SVK", TeamName ="Slovakia", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("BRA", new Team() { TeamID ="BRA", TeamName ="Brazil", NumberColor = Colors.Blue, GroupID ="G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            //TeamsDictionary.Add("PRK", new Team() { TeamID ="PRK", TeamName ="North Korea", NumberColor = Colors.White, GroupID ="G", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("CIV", new Team() { TeamID ="CIV", TeamName ="Ivory Coast", NumberColor = Colors.Blue, GroupID ="G", R1 = 0x80, G1 = 0x70, B1 = 0x00, R2 = 0xC0, G2 = 0xB0, B2 = 0x00, R3 = 0xC0, G3 = 0xB0, B3 = 0x00 });
            //TeamsDictionary.Add("POR", new Team() { TeamID ="POR", TeamName ="Portugal", NumberColor = Colors.White, GroupID ="G", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("ESP", new Team() { TeamID ="ESP", TeamName ="Spain", NumberColor = Colors.White, GroupID ="H", R1 = 0x00, G1 = 0x00, B1 = 0x00, R2 = 0x00, G2 = 0x00, B2 = 0x00, R3 = 0x00, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("SUI", new Team() { TeamID ="SUI", TeamName ="Switzerland", NumberColor = Colors.White, GroupID ="H", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
            //TeamsDictionary.Add("HON", new Team() { TeamID ="HON", TeamName ="Honduras", NumberColor = Colors.White, GroupID ="A", R1 = 0x00, G1 = 0x00, B1 = 0x40, R2 = 0x00, G2 = 0x00, B2 = 0x8D, R3 = 0x00, G3 = 0x00, B3 = 0x8D });
            //TeamsDictionary.Add("CHI", new Team() { TeamID ="CHI", TeamName ="Chile", NumberColor = Colors.White, GroupID ="H", R1 = 0x40, G1 = 0x00, B1 = 0x00, R2 = 0x8D, G2 = 0x00, B2 = 0x00, R3 = 0x8D, G3 = 0x00, B3 = 0x00 });
        }
    }
}
