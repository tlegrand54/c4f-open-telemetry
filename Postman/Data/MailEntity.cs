using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Postman.Data
{
    [Table("Mails")]
    public sealed class MailEntity
    {
        public Guid Id { get; set; }
        public string Receiver { get; set; }
        public int Address { get; set; }
        public string Message { get; set; }
    }
}
