using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static class GameConfig
    {
        public static string roomKey;
        public static string email;
        public static int maxPlayers;
        public static AgentBehaviour agentsBehaviour;
        public static string roomkey;
    }

    public static class LocalPlayerInfo
    {
        public static string role;
        public static string name;
    }

    public enum AgentBehaviour
    {
        Idle, SemiActive, Active, VeryActive
    }

    public enum GameAct
    {
        Day, Vote, Night
    }
}
