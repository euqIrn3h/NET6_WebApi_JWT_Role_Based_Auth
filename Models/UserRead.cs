namespace NET6_WebApi_JWT_Role_Based_Auth.Models
{
    public class UserRead
    {
        public int Id { get; set;}
        public string UserName { get; set; }
        public int Role { get; set; }
    }
}