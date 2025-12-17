using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Requests.PlayerRequests;

public class UpdatePlayerRequest
{
    //TODO make sure the ui tells the user to contact admin if they need to change name/email
    //keeping it to just phone for now. (only required cause its the only field)
    
    [Required, Phone, MaxLength(15)]
    public required string PhoneNumber { get; set; }
}