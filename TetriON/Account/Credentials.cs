namespace TetriON.Account;

public class Credentials {
    
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    
    public Credentials(string username, string password, string email, string token) {
        Username = username;
        Password = password;
        Email = email;
        Token = token;
    }
    
    public Credentials(string username, string password) {
        Username = username;
        Password = password;
    }
    
    public Credentials(string username) {
        Username = username;
    }
    
    
}