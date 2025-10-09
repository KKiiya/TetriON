namespace TetriON.Account;

public class Credentials {
    
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    
    public Credentials(string username, string email, string token) {
        Username = username;
        Email = email;
        Token = token;
    }

    public Credentials(string username, string email) {
        Username = username;
        Email = email;
    }
    
    public Credentials(string username) {
        Username = username;
    }
    
    
}