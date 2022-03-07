using System.ComponentModel.DataAnnotations;

namespace Api
{
    public class UserUpdateModel
    {
        public string Id { get; set; }      

        [MaxLength(50)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }    
    }
}
