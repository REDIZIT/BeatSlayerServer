﻿using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Models.Maps;
using BeatSlayerServer.Utils;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Multiplayer
{
    public class LobbyDTO
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Password { get; set; }
        public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

        public BasicMapData SelectedMap { get; set; }
        public DifficultyData SelectedDifficulty { get; set; }
        public bool IsHostChangingMap { get; set; }

        public List<LobbyPlayerDTO> Players { get; set; }

        public LobbyDTO(Lobby lobby)
        {
            Name = lobby.LobbyName;
            Id = lobby.LobbyId;
            Password = lobby.Password;

            Players = new List<LobbyPlayerDTO>();
            foreach (var player in lobby.Players)
            {
                Players.Add(new LobbyPlayerDTO(player.Value));
            }

            IsHostChangingMap = lobby.IsHostChangingMap;
            SelectedMap = lobby.SelectedMap;
            SelectedDifficulty = lobby.SelectedDifficulty;
        }
    }
}
