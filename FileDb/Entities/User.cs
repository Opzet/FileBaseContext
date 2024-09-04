﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace FileDb
{
    [DataContract]
    public class User : Base
    {
        public User()
        {
            Test2 = Guid.NewGuid();
        }

        [DataMember]
        public string Username { get; set; }

        public string Name { get; set; }

        public int? Test { get; set; }

        [NotMapped]
        public string Ignored { get; set; }

        public UserType Type { get; set; }
        
        public Guid Test2 { get; set; }

        [NotMapped]
        public string[] VContents
        {
            get
            {
                if(Contents != null)
                {
                    return Contents.Select(c => c.Text).ToArray();
                }
                else
                {
                    return new string[] { };
                }
                
            }
        }

        public virtual List<Content> Contents { get; set; }

        public virtual List<Setting> Settings { get; set; }

        public enum UserType
        {
            Admin, User, Manager
        }

        public string Comment { get; set; }
    }
}