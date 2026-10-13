namespace BrasfootAPI.Models;

public class Player
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public int Age {get; set;}
    public int Skill {get; set;}
    public int TeamId {get; set;}
    public string Position {get; set;} = "";
}