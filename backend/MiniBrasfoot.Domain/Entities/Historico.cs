namespace BrasfootAPI.Models;
public class Historico
{
    public int Id { get; set; }
    public int Team1Id {get; set;}
    public int Team2Id {get; set;}
    public int Team1Goals { get; set; }
    public int Team2Goals { get; set; }
    public DateTime DataJogo {get; set;} = DateTime.Now;
}