﻿using System.ComponentModel.DataAnnotations.Schema;

namespace FileDb
{
    public class ContentEntry : Base
    {
        public string Text { get; set; }

        [ForeignKey("Content")]
        public int ContentId { get; set; }

        public virtual Content Content { get; set; }
    }
}
