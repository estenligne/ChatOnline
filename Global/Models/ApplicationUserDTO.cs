using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Global.Models
{
    public class ApplicationUserDTO
    {
        public long Id { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [EmailAddress]
        public string Email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [Phone]
        public string PhoneNumber { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Used only on SignIn HTTP Request
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Used only on SignIn HTTP Response
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Authorization { get; set; }
    }
}
