using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class Account
    {
        public int Account_Id { get; set; }
        public int User_Id { get; set; }
        public int Account_Type_Id { get; set; }
        public int Balance { get; set; }
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
