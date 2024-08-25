﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PPgram_desktop.MVVM.Model.DTO;

internal class ChatDTO
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("username")]
    public string? Username { get; set; }
    [JsonPropertyName("chat_id")]
    public int? Id { get; set; }
    [JsonPropertyName("photo")]
    public string? Photo {  get; set; }
}
