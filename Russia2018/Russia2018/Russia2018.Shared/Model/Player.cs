using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Russia2018.Model
{
    public class Player : Discoid
    {
        #region attributes

        #endregion attributes

        #region constructor

        public Player(SoccerTeam team, int number) : base(20, 1.0, false)
        {
            this.Id = number.ToString("00");
            this.Team = team;
            this.IsPlaying = false;
            friction = 0.025F;
            position = new Vector2D(0, 0);
        }

        #endregion constructor

        #region properties

        public SoccerTeam Team { get; set; }

        public bool IsPlaying { get; set; }

        #endregion properties

        #region functions

        #endregion functions
    }
}

