﻿using System.ComponentModel.DataAnnotations.Schema;

namespace FileDb
{
    public class Setting : Base
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
