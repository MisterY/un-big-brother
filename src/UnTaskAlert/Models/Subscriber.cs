﻿using System;

namespace UnTaskAlert.Models
{
    public class Subscriber
    {
        public TimeSpan StartWorkingHoursUtc { get; set; }
        public TimeSpan EndWorkingHoursUtc { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName="id")]
        public string TelegramId { get; set; }
        public string Email { get; set; }
        public int HoursPerDay { get; set; }
    }
}
