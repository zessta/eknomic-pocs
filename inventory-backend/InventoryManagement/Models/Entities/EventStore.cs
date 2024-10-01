﻿using InventoryManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models.Entities
{
    public class EventStore
    {
        [Key]
        public int EventId {  get; set; }
        public InventoryEvents EventType { get; set; }
        public string? EventData { get; set; }
        public DateTime OccuredTimeUtc { get; set; } = DateTime.UtcNow;
        public required string TriggeredBy { get; set; }
    }
}
