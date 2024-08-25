﻿using System.Text.Json.Serialization;

namespace PPgram_desktop.MVVM.Model.DTO
{
    internal class MessageDTO
    {
        [JsonPropertyName("message_id")]
        public int? Id { get; set; }
        [JsonPropertyName("chat_id")]
        public int? ChatId { get; set; }
        [JsonPropertyName("from_id")]
        public int? From { get; set; }
        [JsonPropertyName("date")]
        public long? Date { get; set; }
        [JsonPropertyName("content")]
        public string? Text { get; set; }
        [JsonPropertyName("reply_to")]
        public int? ReplyTo { get; set; }
        [JsonPropertyName("is_unread")]
        public bool? Unread { get; set; }
        [JsonPropertyName("media_names")]
        public string[]? MediaNames { get; set; }
        [JsonPropertyName("media_datas")]
        public string[]? MediaDatas { get; set; }
    }
}
