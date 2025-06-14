namespace Eshop.Application.DTOs
{
    public class AuthResponseDTO
    {
        public string Message { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool Succeeded { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime ExpireTime { get; set; }
        public List<string> Errors { get; set; }
        public string AccountStatus { get; set; }
        public bool IsBlocked { get; set; }
    }
}
