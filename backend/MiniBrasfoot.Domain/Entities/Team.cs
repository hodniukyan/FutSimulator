namespace BrasfootAPI.Models;
public class Team
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public string Country {get; set;} = "";
    public int Strength {get; set;}
    public bool IsCustom { get; set; }
    public string? BadgeTemplate { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? ModeloDoUniforme { get; set; }
}
