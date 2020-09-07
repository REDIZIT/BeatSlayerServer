using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Utils;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Multiplayer
{
    public class LobbyDTO
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public List<LobbyPlayerDTO> Players { get; set; }

        public MapData SelectedMap { get; set; }


        public LobbyDTO(Lobby lobby)
        {
            Name = lobby.LobbyName;
            Id = lobby.LobbyId;

            Players = new List<LobbyPlayerDTO>();
            foreach (var player in lobby.Players)
            {
                Players.Add(new LobbyPlayerDTO(player.Value));
            }

            SelectedMap = lobby.SelectedMap;
        }
    }
}
